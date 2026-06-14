using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Models;
using SheSecure.Safety_WellnessService.Data;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;





namespace SheSecure.Safety_WellnessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodController : ControllerBase
    {
        private readonly WellnessDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MoodController(
            WellnessDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        private (string employeeId, string email, string role) GetUserContext()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return ("1", "", "Employee");
            }

            var employeeId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("nameid")?.Value
                ?? "1";

            var email = user.FindFirst(ClaimTypes.Name)?.Value
                ?? user.FindFirst("name")?.Value
                ?? "";

            var role = user.FindFirst(ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value
                ?? "Employee";

            return (employeeId, email, role);
        }

        private void ValidateOwnership(string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager")
            {
                if (employeeId != userId && employeeId != email)
                {
                    throw new UnauthorizedAccessException("You are not authorized to access this data.");
                }
            }
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> Create(CreateMoodLogDTO dto)
        {
            var mood = new MoodLog
            {
                EmployeeId = dto.EmployeeId,
                MoodLevel = dto.MoodLevel,
                StressLevel = dto.StressLevel,
                Remarks = dto.Remarks
            };

            _context.MoodLogs.Add(mood);

            await _context.SaveChangesAsync();

            return Ok(mood);
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var (userId, email, role) = GetUserContext();
            if (role == "HR" || role == "Admin" || role == "Manager")
            {
                return Ok(await _context.MoodLogs.ToListAsync());
            }
            else
            {
                return Ok(await _context.MoodLogs
                    .Where(x => x.EmployeeId == userId || x.EmployeeId == email)
                    .ToListAsync());
            }
        }

        [HttpGet("burnout-risk/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetBurnoutRisk(string employeeId)
        {
            try
            {
                ValidateOwnership(employeeId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }

            var logs = await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync();

            if (!logs.Any())
                return NotFound("No mood logs found for this employee.");

            var avgMood = logs.Average(x => x.MoodLevel);
            var avgStress = logs.Average(x => x.StressLevel);

            string riskLevel;

            if (avgMood <= 2 && avgStress >= 4)
                riskLevel = "High";
            else if (avgMood <= 3 && avgStress >= 3)
                riskLevel = "Medium";
            else
                riskLevel = "Low";

            var result = new BurnoutRiskDTO
            {
                EmployeeId = employeeId,
                AverageMood = avgMood,
                AverageStress = avgStress,
                RiskLevel = riskLevel
            };

            return Ok(result);
        }

        [HttpGet("trend/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetMoodTrend(string employeeId)
        {
            try
            {
                ValidateOwnership(employeeId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }

            var logs = await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId)
                .OrderBy(x => x.CreatedAt)
                .Select(x => new MoodTrendDTO
                {
                    Date = x.CreatedAt,
                    MoodLevel = x.MoodLevel,
                    StressLevel = x.StressLevel
                })
                .ToListAsync();

            if (!logs.Any())
                return NotFound("No mood logs found.");

            return Ok(logs);
        }

        [HttpGet("recommendation/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetRecommendation(string employeeId)
        {
            try
            {
                ValidateOwnership(employeeId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }

            var logs = await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId)
                .ToListAsync();

            if (!logs.Any())
                return NotFound("No mood logs found.");

            var avgMood = logs.Average(x => x.MoodLevel);
            var avgStress = logs.Average(x => x.StressLevel);

            string recommendation;

            if (avgMood <= 2 && avgStress >= 4)
            {
                recommendation = "High stress detected. Consider taking a wellness break and speaking with HR.";
            }
            else if (avgMood <= 3 && avgStress >= 3)
            {
                recommendation = "Moderate stress detected. Maintain work-life balance and monitor your wellbeing.";
            }
            else
            {
                recommendation = "You appear to be doing well. Keep maintaining healthy habits.";
            }

            return Ok(new WellnessRecommendationDTO
            {
                EmployeeId = employeeId,
                Recommendation = recommendation
            });
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetEmployeeLogs(string employeeId)
        {
            try
            {
                ValidateOwnership(employeeId);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }

            var logs = await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            return Ok(logs);
        }
    }
}