namespace SheSecure.Safety_WellnessService.DTOs.Requests
{
    public class CreateEmergencyAlertDTO
    {
        public string EmployeeId { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;
    }
}