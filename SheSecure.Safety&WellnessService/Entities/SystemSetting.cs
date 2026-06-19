using System.ComponentModel.DataAnnotations;

namespace SheSecure.Safety_WellnessService.Entities
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public string SettingValue { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
