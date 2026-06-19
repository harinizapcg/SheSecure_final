using System.ComponentModel.DataAnnotations;

namespace SheSecure.Safety_WellnessService.Entities
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string Action { get; set; } = string.Empty;

        public string Entity { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;
    }
}
