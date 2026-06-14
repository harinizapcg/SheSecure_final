using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Data;

namespace SheSecure.Safety_WellnessService.Repositories
{
    public class SafeReachRepository : ISafeReachRepository
    {
        private readonly WellnessDbContext _context;

        public SafeReachRepository(
            WellnessDbContext context)
        {
            _context = context;
        }

        public async Task<SafeReachCheck>
            CreateAsync(SafeReachCheck check)
        {
            _context.SafeReachChecks.Add(check);

            await _context.SaveChangesAsync();

            return check;
        }

        public async Task<List<SafeReachCheck>>
            GetAllAsync()
        {
            return await _context.SafeReachChecks
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<SafeReachCheck?>
            GetByIdAsync(int id)
        {
            return await _context.SafeReachChecks
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateAsync(
            SafeReachCheck check)
        {
            _context.SafeReachChecks.Update(check);

            await _context.SaveChangesAsync();
        }

        public async Task<List<SafeReachCheck>>
            GetByEmployeeAsync(string employeeId)
        {
            return await _context.SafeReachChecks
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}