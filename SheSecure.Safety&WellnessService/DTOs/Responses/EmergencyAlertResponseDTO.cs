namespace SheSecure.Safety_WellnessService.DTOs.Responses
{
    public class EmergencyAlertResponseDTO
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime TriggeredAt { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}