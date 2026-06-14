namespace SheSecure.NotificationService.DTOs.Responses
{
    public class NotificationResponseDTO
    {
        public int Id { get; set; }

        public string EmployeeId { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}