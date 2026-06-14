using Microsoft.EntityFrameworkCore;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Repositories
{
    public class ComplaintStatusHistoryRepository
        : IComplaintStatusHistoryRepository
    {
        private readonly ComplaintDbContext _context;

        public ComplaintStatusHistoryRepository(
            ComplaintDbContext context)
        {
            _context = context;
        }

        public async Task<ComplaintStatusHistory>
            AddHistoryAsync(
                ComplaintStatusHistory history)
        {
            _context.ComplaintStatusHistories.Add(history);

            await _context.SaveChangesAsync();

            return history;
        }

        public async Task<List<ComplaintStatusHistory>>
            GetHistoryByComplaintIdAsync(
                int complaintId)
        {
            return await _context
                .ComplaintStatusHistories
                .Where(x => x.ComplaintId == complaintId)
                .OrderByDescending(x => x.ChangedAt)
                .ToListAsync();
        }
    }
}