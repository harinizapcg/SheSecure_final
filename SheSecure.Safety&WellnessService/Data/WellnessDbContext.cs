using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Models;
using SheSecure.WellnessSafetyService.Entities;

namespace SheSecure.Safety_WellnessService.Data
{
    public class WellnessDbContext : DbContext
    {
        public WellnessDbContext(DbContextOptions<WellnessDbContext> options)
            : base(options)
        {
        }

        public DbSet<SafeReachCheck> SafeReachChecks { get; set; }

        public DbSet<WellnessRequest> WellnessRequests { get; set; }

        public DbSet<EmergencyAlert> EmergencyAlerts { get; set; }

        public DbSet<MoodLog> MoodLogs { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<SystemSetting>().HasData(
                new SystemSetting
                {
                    Id = 1,
                    SettingKey = "SafeReach_Escalation_Minutes",
                    SettingValue = "30",
                    Description = "Number of minutes to wait before escalating an unacknowledged Safe Check-in."
                },
                new SystemSetting
                {
                    Id = 2,
                    SettingKey = "Admin_Alert_Email",
                    SettingValue = "admin@shesecure.com",
                    Description = "Global admin email address to receive critical SOS alerts."
                }
            );
        }
    }
}