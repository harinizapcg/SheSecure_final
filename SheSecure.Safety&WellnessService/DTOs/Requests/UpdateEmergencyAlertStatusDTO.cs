namespace SheSecure.Safety_WellnessService.DTOs.Requests
{
    public class UpdateEmergencyAlertStatusDTO
    {
        public int AlertId { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}