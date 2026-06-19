namespace SheSecure.Safety_WellnessService.DTOs.Requests
{
    public class ResolveEmergencyAlertDTO
    {
        public int AlertId { get; set; }
        public string ResolutionNotes { get; set; } = string.Empty;
    }
}