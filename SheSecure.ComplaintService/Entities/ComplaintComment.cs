using System.ComponentModel.DataAnnotations;

namespace SheSecure.ComplaintService.Entities
{
    public class ComplaintComment
    {
        [Key]
        public int Id { get; set; }

        public int ComplaintId { get; set; }

        public int UserId { get; set; }

        public string Comment { get; set; }

        public bool IsInternal { get; set; }

        public DateTime CreatedAt { get; set; }
            = DateTime.UtcNow;
    }
}