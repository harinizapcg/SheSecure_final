namespace SheSecure.NotificationService.DTOs.Requests
{
    public class CreateNotificationDTO
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}