namespace SheSecure.DashboardService.DTOs
{
    public class DashboardStatsDTO
    {
        public int TotalComplaints { get; set; }

        public int OpenComplaints { get; set; }

        public int ResolvedComplaints { get; set; }

        public int WellnessRequests { get; set; }

        public int ActiveEmergencyAlerts { get; set; }

        public int NotificationsSent { get; set; }
    }
}