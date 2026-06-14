namespace SheSecure.Safety_WellnessService.DTOs
{
    public class BurnoutRiskDTO
    {
        public string EmployeeId { get; set; }

        public double AverageMood { get; set; }

        public double AverageStress { get; set; }

        public string RiskLevel { get; set; }
    }
}