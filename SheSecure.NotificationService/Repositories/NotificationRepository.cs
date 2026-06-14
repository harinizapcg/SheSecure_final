using Microsoft.EntityFrameworkCore;
using SheSecure.NotificationService.Data;
using SheSecure.NotificationService.Entities;
using SheSecure.NotificationService.Interfaces;

namespace SheSecure.NotificationService.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(
            NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<Notification> CreateNotificationAsync(
            Notification notification)
        {
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            return notification;
        }

        public async Task<List<Notification>> GetAllNotificationsAsync()
        {
            return await _context.Notifications
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetEmployeeNotificationsAsync(
     string employeeId)
        {
            return await _context.Notifications
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(
            int id)
        {
            return await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateNotificationAsync(
            Notification notification)
        {
            _context.Notifications.Update(notification);

            await _context.SaveChangesAsync();
        }
    }
}