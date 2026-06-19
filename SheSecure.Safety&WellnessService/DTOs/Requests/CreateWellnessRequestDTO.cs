namespace SheSecure.WellnessSafetyService.DTOs.Requests
{
    public class CreateWellnessRequestDTO
    {
        public string EmployeeId { get; set; }

        public string RequestType { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public DateTime? RequestDate { get; set; }
    }
}