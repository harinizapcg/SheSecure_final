using Microsoft.EntityFrameworkCore;
using SheSecure.NotificationService.Entities;

namespace SheSecure.NotificationService.Data
{
    public class NotificationDbContext : DbContext
    {
        public NotificationDbContext(
            DbContextOptions<NotificationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Notification> Notifications { get; set; }
    }
}