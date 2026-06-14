namespace SheSecure.Safety_WellnessService.DTOs
{
    public class CreateMoodLogDTO
    {
        public string EmployeeId { get; set; }

        public int MoodLevel { get; set; }

        public int StressLevel { get; set; }

        public string? Remarks { get; set; }
    }

    public class MoodLogResponseDTO
    {
        public int Id { get; set; }

        public string EmployeeId { get; set; }

        public int MoodLevel { get; set; }

        public int StressLevel { get; set; }

        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsBurnoutFlagged { get; set; }
    }

    public class BurnoutScoreDTO
    {
        public string EmployeeId { get; set; }

        public double AvgMoodLevel { get; set; }

        public double AvgStressLevel { get; set; }

        // 0 - 100
        public int BurnoutScore { get; set; }

        public bool IsBurnoutRisk { get; set; }

        // "Low" / "Moderate" / "High" / "Critical"
        public string RiskLevel { get; set; }

        public string Message { get; set; }

        public int LogsAnalyzed { get; set; }
    }
}