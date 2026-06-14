using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Data;

namespace SheSecure.Safety_WellnessService.Repositories
{
    public class EmergencyAlertRepository : IEmergencyAlertRepository
    {
        private readonly WellnessDbContext _context;

        public EmergencyAlertRepository(
            WellnessDbContext context)
        {
            _context = context;
        }

        public async Task<EmergencyAlert> CreateAlertAsync(
            EmergencyAlert alert)
        {
            _context.EmergencyAlerts.Add(alert);

            await _context.SaveChangesAsync();

            return alert;
        }

        public async Task<List<EmergencyAlert>> GetAllAlertsAsync()
        {
            return await _context.EmergencyAlerts
                .OrderByDescending(x => x.TriggeredAt)
                .ToListAsync();
        }

        public async Task<EmergencyAlert?> GetByIdAsync(int id)
        {
            return await _context.EmergencyAlerts
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAlertAsync(
            EmergencyAlert alert)
        {
            _context.EmergencyAlerts.Update(alert);

            await _context.SaveChangesAsync();
        }
    }
}