namespace SheSecure.Safety_WellnessService.Models
{
    public class MoodLog
    {
        public int Id { get; set; }

        public string EmployeeId { get; set; }

        public int MoodLevel { get; set; }

        public int StressLevel { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;





    }
}