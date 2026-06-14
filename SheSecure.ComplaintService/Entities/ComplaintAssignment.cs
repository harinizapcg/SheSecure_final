using System.ComponentModel.DataAnnotations;

namespace SheSecure.ComplaintService.Entities
{
    public class ComplaintAssignment
    {
        [Key]
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int AssignedTo { get; set; }

        public int AssignedBy { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}