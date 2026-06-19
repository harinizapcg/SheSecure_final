using System.ComponentModel.DataAnnotations;

namespace SheSecure.WellnessSafetyService.Entities
{
    public class WellnessRequest
    {
        [Key]
        public int Id { get; set; }

        public string EmployeeId { get; set; }

        public string RequestType { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }
            = "Pending";

        public int? AssignedTo { get; set; }

        public DateTime? RequestDate { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}