using SheSecure.Safety_WellnessService.Data;
using System.Text;
using System.Text.Json;

namespace SheSecure.Safety_WellnessService.Jobs
{
    public class SafeReachReminderJob
    {
        private readonly WellnessDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public SafeReachReminderJob(
            WellnessDbContext context,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // Job 1 — Runs at ExpectedArrivalTime
        // Sends reminder, status stays Pending
        public async Task SendReminderAsync(int safeReachId)
        {
            var check = await _context.SafeReachChecks
                .FindAsync(safeReachId);

            // Only act if still pending
            if (check == null || check.Status != "Pending")
                return;

            await SendNotificationAsync(
                employeeId: check.EmployeeId,
                title: "⏰ Safe Arrival Reminder",
                message: "Please confirm that you have reached your destination safely.",
                type: "Safety"
            );

            // Status remains Pending per spec
        }

        // Job 2 — Runs 30 min after ExpectedArrivalTime
        // Escalates if not acknowledged
        public async Task CheckAndEscalateAsync(int safeReachId)
        {
            var check = await _context.SafeReachChecks
                .FindAsync(safeReachId);

            // Already acknowledged — no action needed
            if (check == null || check.IsAcknowledged)
                return;

            // Update status to Escalated
            check.Status = "Escalated";
            _context.SafeReachChecks.Update(check);
            await _context.SaveChangesAsync();

            var message =
                $"Employee ID {check.EmployeeId} failed to confirm safe arrival. Please review immediately.";

            // Notify HR team
            await SendNotificationAsync(
                employeeId: "2",
                title: "🚨 SafeReach Escalation — HR Action Required",
                message: message,
                type: "Emergency"
            );

            // Notify Security team
            await SendNotificationAsync(
                employeeId: "3",
                title: "🚨 SafeReach Escalation — Security Alert",
                message: message,
                type: "Emergency"
            );

            // Notify Admin
            await SendNotificationAsync(
                employeeId: "1",
                title: "🚨 SafeReach Escalation — Admin Notice",
                message: message,
                type: "Emergency"
            );
        }

        private async Task SendNotificationAsync(
            string employeeId, string title,
            string message, string type)
        {
            try
            {
                var client = _httpClientFactory
                    .CreateClient("NotificationService");

                var payload = JsonSerializer.Serialize(new
                {
                    employeeId,
                    title,
                    message,
                    type
                });

                var content = new StringContent(
                    payload, Encoding.UTF8, "application/json");

                await client.PostAsync(
                    "api/Notification/create", content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"[SafeReachReminderJob] Notification failed: {ex.Message}");
            }
        }
    }
}