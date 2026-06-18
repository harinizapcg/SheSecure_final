//using SheSecure.Safety_WellnessService.DTOs.Requests;
//using SheSecure.Safety_WellnessService.Entities;
//using SheSecure.Safety_WellnessService.Interfaces;

//namespace SheSecure.Safety_WellnessService.Services
//{
//    public class EmergencyAlertService : IEmergencyAlertService
//    {
//        private readonly IEmergencyAlertRepository _repository;

//        public EmergencyAlertService(
//            IEmergencyAlertRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task<EmergencyAlert> CreateAlertAsync(
//            CreateEmergencyAlertDTO dto)
//        {
//            var alert = new EmergencyAlert
//            {
//                EmployeeId = dto.EmployeeId,
//                Location = dto.Location,
//                Description = dto.Description,
//                Severity = dto.Severity,
//                Status = "Active",
//                TriggeredAt = DateTime.UtcNow
//            };

//            return await _repository.CreateAlertAsync(alert);
//        }

//        public async Task<List<EmergencyAlert>> GetAllAlertsAsync()
//        {
//            return await _repository.GetAllAlertsAsync();
//        }

//        public async Task<EmergencyAlert?> GetAlertByIdAsync(
//            int id)
//        {
//            return await _repository.GetByIdAsync(id);
//        }

//        public async Task ResolveAlertAsync(
//            ResolveEmergencyAlertDTO dto)
//        {
//            var alert =
//                await _repository.GetByIdAsync(dto.AlertId);

//            if (alert == null)
//                throw new Exception("Alert not found");

//            alert.Status = "Resolved";
//            alert.ResolvedAt = DateTime.UtcNow;

//            await _repository.UpdateAlertAsync(alert);
//        }
//    }
//}
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SheSecure.Safety_WellnessService.DTOs.Requests;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;

namespace SheSecure.Safety_WellnessService.Services
{
    public class EmergencyAlertService : IEmergencyAlertService
    {
        private readonly IEmergencyAlertRepository _repository;
        private readonly HttpClient _http;
        private readonly ILogger<EmergencyAlertService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public EmergencyAlertService(
            IEmergencyAlertRepository repository,
            IHttpClientFactory httpFactory,
            ILogger<EmergencyAlertService> logger,
            IHttpContextAccessor httpContextAccessor,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _repository = repository;
            _http = httpFactory.CreateClient("NotificationService");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _configuration = configuration;
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

        public async Task<EmergencyAlert> CreateAlertAsync(
            CreateEmergencyAlertDTO dto)
        {
            var alert = new EmergencyAlert
            {
                EmployeeId = dto.EmployeeId,
                Location = dto.Location,
                Description = dto.Description,
                Severity = dto.Severity,
                Status = "Active",
                TriggeredAt = DateTime.UtcNow
            };

            var created =
                await _repository.CreateAlertAsync(alert);

            await SendNotificationAsync(
                dto.EmployeeId.ToString(),
                "Emergency SOS Raised",
                "Your SOS alert has been triggered. Help is on the way.",
                "SOS_RAISED");

            await SendAdminEmailNotificationAsync(created, dto);

            return created;
        }

        private async Task SendAdminEmailNotificationAsync(EmergencyAlert alert, CreateEmergencyAlertDTO dto)
        {
            try
            {
                var adminEmail = _configuration["AdminSettings:AdminEmail"] ?? "admin@shesecure.com";
                var subject = "🚨 SOS Alert Triggered";
                
                var (userId, userEmail, role) = GetUserContext();
                
                var body = $@"
                    <h2>SOS Alert Triggered</h2>
                    <p><strong>Immediate attention is required!</strong></p>
                    <ul>
                        <li><strong>Alert ID:</strong> {alert.Id}</li>
                        <li><strong>Creation Time:</strong> {alert.TriggeredAt:yyyy-MM-dd HH:mm:ss UTC}</li>
                        <li><strong>User ID:</strong> {userId}</li>
                        <li><strong>User Email:</strong> {userEmail}</li>
                        <li><strong>Location:</strong> {alert.Location}</li>
                        <li><strong>Severity:</strong> {alert.Severity}</li>
                        <li><strong>Description:</strong> {alert.Description}</li>
                    </ul>
                    <p>Please log in to the dashboard to take action immediately.</p>
                ";

                await _emailService.SendEmailAsync(adminEmail, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SOS email notification for Alert {AlertId}", alert.Id);
            }
        }

        public async Task<List<EmergencyAlert>>
            GetAllAlertsAsync()
        {
            var (userId, email, role) = GetUserContext();
            var alerts = await _repository.GetAllAlertsAsync();

            if (role == "Security" || role == "Admin" || role == "Manager")
            {
                return alerts;
            }
            else
            {
                return alerts.Where(x => x.EmployeeId == userId || x.EmployeeId == email).ToList();
            }
        }

        public async Task<EmergencyAlert?> GetAlertByIdAsync(
            int id)
        {
            var alert = await _repository.GetByIdAsync(id);
            if (alert == null)
                return null;

            var (userId, email, role) = GetUserContext();
            if (role != "Security" && role != "Admin" && role != "Manager" && alert.EmployeeId != userId && alert.EmployeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this emergency alert.");
            }

            return alert;
        }

        public async Task ResolveAlertAsync(
            ResolveEmergencyAlertDTO dto)
        {
            var alert =
                await _repository.GetByIdAsync(dto.AlertId);

            if (alert == null)
                throw new Exception("Alert not found");

            alert.Status = "Resolved";
            alert.ResolvedAt = DateTime.UtcNow;

            await _repository.UpdateAlertAsync(alert);

            await SendNotificationAsync(
                alert.EmployeeId.ToString(),
                "SOS Alert Resolved",
                "Your emergency alert has been marked as resolved.",
                "SOS_RESOLVED");
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