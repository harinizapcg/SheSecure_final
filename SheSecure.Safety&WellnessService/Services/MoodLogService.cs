using Microsoft.AspNetCore.Http;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Models;

namespace SheSecure.Safety_WellnessService.Services
{
    public class MoodLogService : IMoodLogService
    {
        private readonly IMoodLogRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Look back 7 days for burnout calculation
        private const int LookbackDays = 7;

        // Score >= 60 is considered burnout risk
        private const int BurnoutRiskThreshold = 60;

        public MoodLogService(
            IMoodLogRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _httpContextAccessor = httpContextAccessor;
        }

        private (string employeeId, string email, string role) GetUserContext()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return ("1", "", "Employee");
            }

            var employeeId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("nameid")?.Value
                ?? "1";

            var email = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? user.FindFirst("name")?.Value
                ?? "";

            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value
                ?? "Employee";

            return (employeeId, email, role);
        }

        public async Task<MoodLogResponseDTO> AddLogAsync(CreateMoodLogDTO dto)
        {
            // Calculate burnout flag before saving
            bool isBurnout = await CheckBurnoutRiskAsync(
                dto.EmployeeId, dto.MoodLevel, dto.StressLevel);

            var log = new MoodLog
            {
                EmployeeId = dto.EmployeeId,
                MoodLevel = dto.MoodLevel,
                StressLevel = dto.StressLevel,
                Remarks = dto.Remarks,
                CreatedAt = DateTime.UtcNow
            };

            var saved = await _repository.AddLogAsync(log);

            return MapToResponse(saved, isBurnout);
        }

        public async Task<List<MoodLogResponseDTO>> GetLogsByEmployeeAsync(
            string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && employeeId != userId && employeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these mood logs.");
            }

            var logs = await _repository.GetByEmployeeIdAsync(employeeId);

            return logs.Select(l => MapToResponse(l, false)).ToList();
        }

        public async Task<BurnoutScoreDTO> GetBurnoutScoreAsync(string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && employeeId != userId && employeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this burnout score.");
            }

            var recentLogs = await _repository
                .GetRecentByEmployeeIdAsync(employeeId, LookbackDays);

            if (!recentLogs.Any())
            {
                return new BurnoutScoreDTO
                {
                    EmployeeId = employeeId,
                    AvgMoodLevel = 0,
                    AvgStressLevel = 0,
                    BurnoutScore = 0,
                    IsBurnoutRisk = false,
                    RiskLevel = "Unknown",
                    Message = "No mood logs found in the last 7 days.",
                    LogsAnalyzed = 0
                };
            }

            double avgMood = recentLogs.Average(l => l.MoodLevel);
            double avgStress = recentLogs.Average(l => l.StressLevel);

            // Burnout score formula (0-100):
            // Low mood (inverted): (5 - avgMood) * 10     → max 40 pts
            // High stress:         (avgStress - 1) * 10   → max 40 pts
            // Consecutive bad days (mood<=2 & stress>=4)  → max 20 pts
            double moodComponent = (5.0 - avgMood) * 10.0;
            double stressComponent = (avgStress - 1.0) * 10.0;

            int consecutiveBadDays = CountConsecutiveBadDays(recentLogs);
            double consistencyBonus = Math.Min(consecutiveBadDays * 5.0, 20.0);

            int burnoutScore = (int)Math.Round(
                moodComponent + stressComponent + consistencyBonus);

            burnoutScore = Math.Clamp(burnoutScore, 0, 100);

            bool isRisk = burnoutScore >= BurnoutRiskThreshold;

            string riskLevel = burnoutScore switch
            {
                < 30 => "Low",
                < 60 => "Moderate",
                < 80 => "High",
                _ => "Critical"
            };

            string message = riskLevel switch
            {
                "Low" => "Employee wellness looks healthy.",
                "Moderate" => "Some stress indicators present. Consider a check-in.",
                "High" => "Elevated burnout risk. HR follow-up recommended.",
                "Critical" => "Critical burnout risk detected. Immediate support needed.",
                _ => ""
            };

            return new BurnoutScoreDTO
            {
                EmployeeId = employeeId,
                AvgMoodLevel = Math.Round(avgMood, 2),
                AvgStressLevel = Math.Round(avgStress, 2),
                BurnoutScore = burnoutScore,
                IsBurnoutRisk = isRisk,
                RiskLevel = riskLevel,
                Message = message,
                LogsAnalyzed = recentLogs.Count
            };
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<bool> CheckBurnoutRiskAsync(
            string employeeId, int todayMood, int todayStress)
        {
            var recentLogs = await _repository
                .GetRecentByEmployeeIdAsync(employeeId, LookbackDays);

            if (!recentLogs.Any())
                return todayMood <= 2 && todayStress >= 4;

            double avgMood = (recentLogs.Sum(l => l.MoodLevel) + todayMood)
                             / (double)(recentLogs.Count + 1);
            double avgStress = (recentLogs.Sum(l => l.StressLevel) + todayStress)
                             / (double)(recentLogs.Count + 1);

            double moodComponent = (5.0 - avgMood) * 10.0;
            double stressComponent = (avgStress - 1.0) * 10.0;

            int score = (int)Math.Round(moodComponent + stressComponent);

            return Math.Clamp(score, 0, 100) >= BurnoutRiskThreshold;
        }

        private int CountConsecutiveBadDays(List<MoodLog> logs)
        {
            int count = 0;

            foreach (var log in logs.OrderByDescending(l => l.CreatedAt))
            {
                if (log.MoodLevel <= 2 && log.StressLevel >= 4)
                    count++;
                else
                    break;
            }

            return count;
        }

        private MoodLogResponseDTO MapToResponse(MoodLog log, bool isBurnout) => new()
        {
            Id = log.Id,
            EmployeeId = log.EmployeeId,
            MoodLevel = log.MoodLevel,
            StressLevel = log.StressLevel,
            Remarks = log.Remarks,
            CreatedAt = log.CreatedAt,
            IsBurnoutFlagged = isBurnout
        };
    }
}