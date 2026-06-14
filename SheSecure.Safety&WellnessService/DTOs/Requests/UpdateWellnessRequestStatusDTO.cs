namespace SheSecure.WellnessSafetyService.DTOs.Requests
{
    public class UpdateWellnessRequestStatusDTO
    {
        public int RequestId { get; set; }

        public string Status { get; set; }

        public int? AssignedTo { get; set; }
    }
}