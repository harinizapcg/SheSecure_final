//using Hangfire;
//using SheSecure.Safety_WellnessService.DTOs;
//using SheSecure.Safety_WellnessService.Entities;
//using SheSecure.Safety_WellnessService.Interfaces;
//using SheSecure.Safety_WellnessService.Jobs;

//namespace SheSecure.Safety_WellnessService.Services
//{
//    public class SafeReachService : ISafeReachService
//    {
//        private readonly ISafeReachRepository _repository;

//        public SafeReachService(
//            ISafeReachRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task CreateAsync(
//            CreateSafeReachDTO dto)
//        {
//            var check = new SafeReachCheck
//            {
//                EmployeeId = dto.EmployeeId,
//                ExpectedArrivalTime = dto.ExpectedArrivalTime,
//                IsAcknowledged = false,
//                Status = "Pending"
//            };

//            await _repository.CreateAsync(check);

//            // Normalize to UTC
//            var expectedUtc =
//                dto.ExpectedArrivalTime.Kind == DateTimeKind.Utc
//                    ? dto.ExpectedArrivalTime
//                    : dto.ExpectedArrivalTime.ToUniversalTime();

//            var now = DateTime.UtcNow;

//            // Job 1 — reminder at expected arrival time
//            var reminderDelay = expectedUtc - now;
//            if (reminderDelay > TimeSpan.Zero)
//            {
//                BackgroundJob.Schedule<SafeReachReminderJob>(
//                    job => job.SendReminderAsync(check.Id),
//                    reminderDelay);
//            }
//            else
//            {
//                BackgroundJob.Enqueue<SafeReachReminderJob>(
//                    job => job.SendReminderAsync(check.Id));
//            }

//            // Job 2 — escalation 30 min after expected arrival
//            var escalationDelay =
//                expectedUtc - now + TimeSpan.FromMinutes(30);
//            if (escalationDelay > TimeSpan.Zero)
//            {
//                BackgroundJob.Schedule<SafeReachReminderJob>(
//                    job => job.CheckAndEscalateAsync(check.Id),
//                    escalationDelay);
//            }
//            else
//            {
//                BackgroundJob.Schedule<SafeReachReminderJob>(
//                    job => job.CheckAndEscalateAsync(check.Id),
//                    TimeSpan.FromMinutes(30));
//            }
//        }

//        public async Task AcknowledgeAsync(
//            AcknowledgeSafeReachDTO dto)
//        {
//            var check =
//                await _repository.GetByIdAsync(
//                    dto.SafeReachId);

//            if (check == null)
//                throw new Exception(
//                    "Safe Reach record not found");

//            check.IsAcknowledged = true;
//            check.AcknowledgedAt = DateTime.UtcNow;
//            check.Status = "Acknowledged";

//            await _repository.UpdateAsync(check);
//        }

//        public async Task EscalateAsync(int id)
//        {
//            var check =
//                await _repository.GetByIdAsync(id);

//            if (check == null)
//                throw new Exception(
//                    "Safe Reach record not found");

//            if (check.IsAcknowledged)
//                throw new Exception(
//                    "Employee already acknowledged");

//            check.Status = "Escalated";
//            await _repository.UpdateAsync(check);
//        }

//        public async Task<object> GetAllAsync()
//            => await _repository.GetAllAsync();

//        public async Task<object> GetByIdAsync(int id)
//        {
//            var check =
//                await _repository.GetByIdAsync(id);

//            if (check == null)
//                throw new Exception(
//                    "Safe Reach record not found");

//            return check;
//        }
//    }
//}
using System.Text;
using System.Text.Json;
using Hangfire;
using Microsoft.AspNetCore.Http;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Jobs;

namespace SheSecure.Safety_WellnessService.Services
{
    public class SafeReachService : ISafeReachService
    {
        private readonly ISafeReachRepository _repository;
        private readonly HttpClient _http;
        private readonly ILogger<SafeReachService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SafeReachService(
            ISafeReachRepository repository,
            IHttpClientFactory httpFactory,
            ILogger<SafeReachService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _http = httpFactory.CreateClient("NotificationService");
            _logger = logger;
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

        public async Task CreateAsync(CreateSafeReachDTO dto)
        {
            var check = new SafeReachCheck
            {
                EmployeeId = dto.EmployeeId,
                ExpectedArrivalTime = dto.ExpectedArrivalTime,
                IsAcknowledged = false,
                Status = "Pending"
            };

            await _repository.CreateAsync(check);

            await SendNotificationAsync(
                dto.EmployeeId.ToString(),
                "Safe Reach Check Scheduled",
                "A safe reach check has been set up. Please acknowledge when you arrive safely.",
                "SAFE_REACH_CREATED");

            var expectedUtc =
                dto.ExpectedArrivalTime.Kind == DateTimeKind.Utc
                    ? dto.ExpectedArrivalTime
                    : dto.ExpectedArrivalTime.ToUniversalTime();

            var now = DateTime.UtcNow;

            var reminderDelay = expectedUtc - now;
            if (reminderDelay > TimeSpan.Zero)
                BackgroundJob.Schedule<SafeReachReminderJob>(
                    job => job.SendReminderAsync(check.Id),
                    reminderDelay);
            else
                BackgroundJob.Enqueue<SafeReachReminderJob>(
                    job => job.SendReminderAsync(check.Id));

            var escalationDelay =
                expectedUtc - now + TimeSpan.FromMinutes(30);
            if (escalationDelay > TimeSpan.Zero)
                BackgroundJob.Schedule<SafeReachReminderJob>(
                    job => job.CheckAndEscalateAsync(check.Id),
                    escalationDelay);
            else
                BackgroundJob.Schedule<SafeReachReminderJob>(
                    job => job.CheckAndEscalateAsync(check.Id),
                    TimeSpan.FromMinutes(30));
        }

        public async Task AcknowledgeAsync(
            AcknowledgeSafeReachDTO dto)
        {
            var check =
                await _repository.GetByIdAsync(dto.SafeReachId);

            if (check == null)
                throw new Exception(
                    "Safe Reach record not found");

            check.IsAcknowledged = true;
            check.AcknowledgedAt = DateTime.UtcNow;
            check.Status = "Acknowledged";

            await _repository.UpdateAsync(check);

            await SendNotificationAsync(
                check.EmployeeId.ToString(),
                "Safe Arrival Confirmed",
                "You have confirmed your safe arrival. Stay safe!",
                "SAFE_REACHED");
        }

        public async Task EscalateAsync(int id)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
                throw new Exception(
                    "Safe Reach record not found");

            if (check.IsAcknowledged)
                throw new Exception(
                    "Employee already acknowledged");

            check.Status = "Escalated";

            await _repository.UpdateAsync(check);

            await SendNotificationAsync(
                check.EmployeeId.ToString(),
                "Safe Reach Escalated",
                "You have not acknowledged your arrival. Your case has been escalated to your manager.",
                "SAFE_REACH_ESCALATED");
        }

        public async Task<object> GetAllAsync()
        {
            var (userId, email, role) = GetUserContext();
            if (role == "HR" || role == "Admin" || role == "Manager")
            {
                return await _repository.GetAllAsync();
            }
            else
            {
                return await _repository.GetByEmployeeAsync(userId);
            }
        }

        public async Task<object> GetByIdAsync(int id)
        {
            var check = await _repository.GetByIdAsync(id);

            if (check == null)
                throw new Exception(
                    "Safe Reach record not found");

            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && check.EmployeeId != userId && check.EmployeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this record.");
            }

            return check;
        }

        public async Task<object> GetByEmployeeAsync(
            string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && employeeId != userId && employeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these records.");
            }

            return await _repository.GetByEmployeeAsync(employeeId);
        }

        private async Task SendNotificationAsync(
            string employeeId,
            string title,
            string message,
            string type)
        {
            try
            {
                var payload = JsonSerializer.Serialize(
                    new { employeeId, title, message, type },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    });

                await _http.PostAsync(
                    "api/Notification/create",
                    new StringContent(
                        payload,
                        Encoding.UTF8,
                        "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send notification [{Type}] " +
                    "for employee {EmployeeId}", type, employeeId);
            }
        }
    }
}