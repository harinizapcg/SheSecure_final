namespace SheSecure.DashboardService.DTOs
{
    /// <summary>
    /// Summary statistics for a single employee's own records.
    /// </summary>
    public class EmployeeDashboardDTO
    {
        public string EmployeeId { get; set; } = string.Empty;

        public int TotalComplaints { get; set; }

        public int TotalWellnessRequests { get; set; }

        public int TotalSafeReachRecords { get; set; }

        public int UnreadNotifications { get; set; }
    }
}
