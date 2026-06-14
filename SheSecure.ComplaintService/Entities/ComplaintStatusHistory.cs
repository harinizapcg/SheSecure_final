using System.ComponentModel.DataAnnotations;

namespace SheSecure.ComplaintService.Entities
{
    public class ComplaintStatusHistory
    {
        [Key]
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public string OldStatus { get; set; }

        public string NewStatus { get; set; }

        public int ChangedBy { get; set; }

        public string Remarks { get; set; }

        public DateTime ChangedAt { get; set; }
            = DateTime.UtcNow;
    }
}