using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SheSecure.ComplaintService.Entities
{
    public class Complaint
    {
        [Key]
        public int Id { get; set; }

        public string ComplaintNumber { get; set; }

        public string? AssignedTo { get; set; }
        public string EmployeeId { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string Category { get; set; }

        public string Subject { get; set; }

        public string Description { get; set; }

        public string Priority { get; set; }

        public string Status { get; set; }

        public bool IsAnonymous { get; set; }

        

        public string? ResolutionNotes { get; set; }



        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        

        public ICollection<ComplaintFile> ComplaintFiles { get; set; }
    }
}