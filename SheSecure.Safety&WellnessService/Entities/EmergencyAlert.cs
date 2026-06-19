using System.ComponentModel.DataAnnotations;

namespace SheSecure.Safety_WellnessService.Entities
{
    public class EmergencyAlert
    {
        [Key]
        public int Id { get; set; }

        public string EmployeeId { get; set; }

        public string Location { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";

        public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public string ResolutionNotes { get; set; } = string.Empty;
    }
}