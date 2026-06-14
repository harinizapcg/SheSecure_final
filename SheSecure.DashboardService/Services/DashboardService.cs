using SheSecure.DashboardService.DTOs;
using SheSecure.DashboardService.Interfaces;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace SheSecure.DashboardService.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardService(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpClient GetClientWithAuth()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            var client = new HttpClient(handler);

            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                client.DefaultRequestHeaders.Add("Authorization", authHeader);
            }
            return client;
        }

        public async Task<DashboardStatsDTO> GetStatsAsync()
        {
            var client = GetClientWithAuth();

            int totalComplaints = 0;
            int openComplaints = 0;
            int resolvedComplaints = 0;
            int wellnessRequests = 0;
            int activeEmergencies = 0;
            int notificationsSent = 0;

            // ── Complaints ────────────────────────────────────────────────
            try
            {
                var complaints = await client
                    .GetFromJsonAsync<List<ComplaintItemDTO>>(
                        $"{ComplaintBase()}/api/Complaint/all");

                if (complaints != null)
                {
                    totalComplaints = complaints.Count;
                    openComplaints = complaints.Count(
                        x => x.Status != "Resolved" && x.Status != "Closed");
                    resolvedComplaints = complaints.Count(
                        x => x.Status == "Resolved" || x.Status == "Closed");
                }
            }
            catch { /* service unavailable — leave as 0 */ }

            // ── Wellness Requests ─────────────────────────────────────────
            try
            {
                var wellness = await client
                    .GetFromJsonAsync<List<WellnessItemDTO>>(
                        $"{WellnessBase()}/api/WellnessRequest/all");

                if (wellness != null)
                    wellnessRequests = wellness.Count;
            }
            catch { }

            // ── Emergency Alerts ──────────────────────────────────────────
            try
            {
                var emergencies = await client
                    .GetFromJsonAsync<List<EmergencyItemDTO>>(
                        $"{WellnessBase()}/api/EmergencyAlert/all");

                if (emergencies != null)
                    activeEmergencies = emergencies.Count(
                        x => x.Status == "Active");
            }
            catch { }

            // ── Notifications ─────────────────────────────────────────────
            try
            {
                var notifications = await client
                    .GetFromJsonAsync<List<NotificationItemDTO>>(
                        $"{NotificationBase()}/api/Notification/all");

                if (notifications != null)
                    notificationsSent = notifications.Count;
            }
            catch { }

            return new DashboardStatsDTO
            {
                TotalComplaints = totalComplaints,
                OpenComplaints = openComplaints,
                ResolvedComplaints = resolvedComplaints,
                WellnessRequests = wellnessRequests,
                ActiveEmergencyAlerts = activeEmergencies,
                NotificationsSent = notificationsSent
            };
        }

        public async Task<List<ComplaintAnalyticsDTO>>
            GetComplaintAnalyticsAsync()
        {
            var client = GetClientWithAuth();

            try
            {
                var complaints = await client
                    .GetFromJsonAsync<List<ComplaintItemDTO>>(
                        $"{ComplaintBase()}/api/Complaint/all");

                if (complaints == null || !complaints.Any())
                    return new List<ComplaintAnalyticsDTO>();

                // Group by Category
                return complaints
                    .GroupBy(x => x.Category ?? "Uncategorized")
                    .Select(g => new ComplaintAnalyticsDTO
                    {
                        Category = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }
            catch
            {
                return new List<ComplaintAnalyticsDTO>();
            }
        }

        public async Task<List<WellnessAnalyticsDTO>>
            GetWellnessAnalyticsAsync()
        {
            var client = GetClientWithAuth();

            try
            {
                var wellness = await client
                    .GetFromJsonAsync<List<WellnessItemDTO>>(
                        $"{WellnessBase()}/api/WellnessRequest/all");

                if (wellness == null || !wellness.Any())
                    return new List<WellnessAnalyticsDTO>();

                // Group by RequestType
                return wellness
                    .GroupBy(x => x.RequestType ?? "General")
                    .Select(g => new WellnessAnalyticsDTO
                    {
                        RequestType = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }
            catch
            {
                return new List<WellnessAnalyticsDTO>();
            }
        }

        public async Task<List<EmergencyAnalyticsDTO>>
            GetEmergencyAnalyticsAsync()
        {
            var client = GetClientWithAuth();

            try
            {
                var emergencies = await client
                    .GetFromJsonAsync<List<EmergencyItemDTO>>(
                        $"{WellnessBase()}/api/EmergencyAlert/all");

                if (emergencies == null || !emergencies.Any())
                    return new List<EmergencyAnalyticsDTO>();

                // Group by Status
                return emergencies
                    .GroupBy(x => x.Status ?? "Unknown")
                    .Select(g => new EmergencyAnalyticsDTO
                    {
                        Status = g.Key,
                        Count = g.Count()
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();
            }
            catch
            {
                return new List<EmergencyAnalyticsDTO>();
            }
        }

        // ── Base URLs from appsettings ────────────────────────────────────
        private string ComplaintBase() =>
            _configuration["ServiceUrls:ComplaintService"]
                ?? "https://localhost:7032";

        private string WellnessBase() =>
            _configuration["ServiceUrls:WellnessService"]
                ?? "https://localhost:7044";

        private string NotificationBase() =>
            _configuration["ServiceUrls:NotificationService"]
                ?? "https://localhost:7179";

        public async Task<EmployeeDashboardDTO>
            GetEmployeeDashboardAsync(string employeeId)
        {
            var client = GetClientWithAuth();

            int totalComplaints = 0;
            int totalWellness = 0;
            int totalSafeReach = 0;
            int unreadNotifications = 0;

            // ── Employee Complaints ───────────────────────────────────────
            try
            {
                var complaints = await client
                    .GetFromJsonAsync<List<ComplaintItemDTO>>(
                        $"{ComplaintBase()}/api/Complaint/by-employee/{employeeId}");

                if (complaints != null)
                    totalComplaints = complaints.Count;
            }
            catch { }

            // ── Employee Wellness Requests ────────────────────────────────
            try
            {
                var wellness = await client
                    .GetFromJsonAsync<List<WellnessItemDTO>>(
                        $"{WellnessBase()}/api/WellnessRequest/by-employee/{employeeId}");

                if (wellness != null)
                    totalWellness = wellness.Count;
            }
            catch { }

            // ── Employee Safe Reach Records ───────────────────────────────
            try
            {
                var safereach = await client
                    .GetFromJsonAsync<List<SafeReachItemDTO>>(
                        $"{WellnessBase()}/api/SafeReach/by-employee/{employeeId}");

                if (safereach != null)
                    totalSafeReach = safereach.Count;
            }
            catch { }

            // ── Employee Unread Notifications ─────────────────────────────
            try
            {
                var notifications = await client
                    .GetFromJsonAsync<List<NotificationItemDTO>>(
                        $"{NotificationBase()}/api/Notification/by-employee/{employeeId}");

                if (notifications != null)
                    unreadNotifications = notifications.Count(
                        x => !x.IsRead);
            }
            catch { }

            return new EmployeeDashboardDTO
            {
                EmployeeId = employeeId,
                TotalComplaints = totalComplaints,
                TotalWellnessRequests = totalWellness,
                TotalSafeReachRecords = totalSafeReach,
                UnreadNotifications = unreadNotifications
            };
        }

        public async Task<AdminDashboardDTO> GetAdminDashboardAsync()
        {
            var client = GetClientWithAuth();

            int totalComplaints = 0;
            int openComplaints = 0;
            int pendingWellness = 0;
            int totalSafeReach = 0;
            int pendingSafeReach = 0;
            int sosAlerts = 0;
            int totalNotifications = 0;
            int unreadNotifications = 0;

            // ── All Complaints ────────────────────────────────────────────
            try
            {
                var complaints = await client
                    .GetFromJsonAsync<List<ComplaintItemDTO>>(
                        $"{ComplaintBase()}/api/Complaint/all");

                if (complaints != null)
                {
                    totalComplaints = complaints.Count;
                    openComplaints = complaints.Count(
                        x => x.Status != "Resolved" && x.Status != "Closed");
                }
            }
            catch { }

            // ── All Wellness Requests ─────────────────────────────────────
            try
            {
                var wellness = await client
                    .GetFromJsonAsync<List<WellnessItemDTO>>(
                        $"{WellnessBase()}/api/WellnessRequest/all");

                if (wellness != null)
                    pendingWellness = wellness.Count(
                        x => x.Status == "Pending");
            }
            catch { }

            // ── All Safe Reach Records ────────────────────────────────────
            try
            {
                var safereach = await client
                    .GetFromJsonAsync<List<SafeReachItemDTO>>(
                        $"{WellnessBase()}/api/SafeReach/all");

                if (safereach != null)
                {
                    totalSafeReach = safereach.Count;
                    pendingSafeReach = safereach.Count(
                        x => x.Status == "Pending");
                }
            }
            catch { }

            // ── SOS / Emergency Alerts ────────────────────────────────────
            try
            {
                var emergencies = await client
                    .GetFromJsonAsync<List<EmergencyItemDTO>>(
                        $"{WellnessBase()}/api/EmergencyAlert/all");

                if (emergencies != null)
                    sosAlerts = emergencies.Count(
                        x => x.Status == "Active");
            }
            catch { }

            // ── All Notifications ─────────────────────────────────────────
            try
            {
                var notifications = await client
                    .GetFromJsonAsync<List<NotificationItemDTO>>(
                        $"{NotificationBase()}/api/Notification/all");

                if (notifications != null)
                {
                    totalNotifications = notifications.Count;
                    unreadNotifications = notifications.Count(
                        x => !x.IsRead);
                }
            }
            catch { }

            return new AdminDashboardDTO
            {
                TotalComplaints = totalComplaints,
                OpenComplaints = openComplaints,
                PendingWellnessRequests = pendingWellness,
                TotalSafeReachRecords = totalSafeReach,
                PendingSafeReach = pendingSafeReach,
                SosAlerts = sosAlerts,
                TotalNotifications = totalNotifications,
                UnreadNotifications = unreadNotifications
            };
        }
    }

    // ── Internal DTOs for deserializing other services' responses ────────
    // These are lightweight — only the fields Dashboard needs

    internal class ComplaintItemDTO
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? Category { get; set; }
    }

    internal class WellnessItemDTO
    {
        public int Id { get; set; }
        public string? RequestType { get; set; }
        public string? Status { get; set; }
    }

    internal class EmergencyItemDTO
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }

    internal class NotificationItemDTO
    {
        public int Id { get; set; }
        public bool IsRead { get; set; }
    }

    internal class SafeReachItemDTO
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }
}