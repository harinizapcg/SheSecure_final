using SheSecure.NotificationService.Entities;

namespace SheSecure.NotificationService.Interfaces
{
    public interface INotificationRepository
    {
        Task<Notification> CreateNotificationAsync(
            Notification notification);

        Task<List<Notification>> GetAllNotificationsAsync();

        Task<List<Notification>> GetEmployeeNotificationsAsync(
            string employeeId);

        Task<Notification?> GetByIdAsync(int id);

        Task UpdateNotificationAsync(
            Notification notification);
    }
}