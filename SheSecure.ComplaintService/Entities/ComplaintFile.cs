using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SheSecure.ComplaintService.Entities
{
    public class ComplaintFile
    {
        [Key]
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public string FileName { get; set; }

        public string FilePath { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ComplaintId")]
        public Complaint Complaint { get; set; }
    }
}