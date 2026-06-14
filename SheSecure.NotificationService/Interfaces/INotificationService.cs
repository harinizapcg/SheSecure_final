using SheSecure.NotificationService.DTOs.Requests;
using SheSecure.NotificationService.DTOs.Responses;

namespace SheSecure.NotificationService.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationResponseDTO>
            CreateNotificationAsync(
                CreateNotificationDTO dto);

        Task<List<NotificationResponseDTO>>
            GetAllNotificationsAsync();

        Task<List<NotificationResponseDTO>>
            GetEmployeeNotificationsAsync(
                string employeeId);

        Task MarkAsReadAsync(
            int notificationId);
    }
}