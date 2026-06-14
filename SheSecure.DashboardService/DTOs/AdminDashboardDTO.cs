namespace SheSecure.DashboardService.DTOs
{
    /// <summary>
    /// Aggregate statistics for admin-level dashboard visibility.
    /// </summary>
    public class AdminDashboardDTO
    {
        public int TotalComplaints { get; set; }

        public int OpenComplaints { get; set; }

        public int PendingWellnessRequests { get; set; }

        public int TotalSafeReachRecords { get; set; }

        public int PendingSafeReach { get; set; }

        public int SosAlerts { get; set; }

        public int TotalNotifications { get; set; }

        public int UnreadNotifications { get; set; }
    }
}
