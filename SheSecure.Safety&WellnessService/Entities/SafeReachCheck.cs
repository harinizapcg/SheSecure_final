namespace SheSecure.Safety_WellnessService.Entities
{
    public class SafeReachCheck
    {
        public int Id { get; set; }
       
        public string EmployeeId { get; set; }

        public DateTime ExpectedArrivalTime { get; set; }

        public bool IsAcknowledged { get; set; }

        public DateTime? AcknowledgedAt { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}