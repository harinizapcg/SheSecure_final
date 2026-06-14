using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Models;
using SheSecure.Safety_WellnessService.Data;

namespace SheSecure.Safety_WellnessService.Repositories
{
    public class MoodLogRepository : IMoodLogRepository
    {
        private readonly WellnessDbContext _context;

        public MoodLogRepository(WellnessDbContext context)
        {
            _context = context;
        }

        public async Task<MoodLog> AddLogAsync(MoodLog log)
        {
            _context.MoodLogs.Add(log);

            await _context.SaveChangesAsync();

            return log;
        }

        public async Task<List<MoodLog>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<MoodLog>> GetRecentByEmployeeIdAsync(
            string employeeId, int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);

            return await _context.MoodLogs
                .Where(x => x.EmployeeId == employeeId
                         && x.CreatedAt >= cutoff)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}