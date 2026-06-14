using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Entities;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.WellnessSafetyService.Entities;
using SheSecure.WellnessSafetyService.Interfaces;
using SheSecure.Safety_WellnessService.Data;

namespace SheSecure.Safety_WellnessService.Repositories
{
    public class WellnessRequestRepository : IWellnessRequestRepository
    {
        private readonly WellnessDbContext _context;

        public WellnessRequestRepository(WellnessDbContext context)
        {
            _context = context;
        }

        public async Task<WellnessRequest> CreateRequestAsync(
            WellnessRequest request)
        {
            _context.WellnessRequests.Add(request);

            await _context.SaveChangesAsync();

            return request;
        }

        public async Task<List<WellnessRequest>> GetAllRequestsAsync()
        {
            return await _context.WellnessRequests
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<WellnessRequest?> GetByIdAsync(int id)
        {
            return await _context.WellnessRequests
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<List<WellnessRequest>> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.WellnessRequests
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateRequestAsync(WellnessRequest request)
        {
            _context.WellnessRequests.Update(request);

            await _context.SaveChangesAsync();
        }
    }
}