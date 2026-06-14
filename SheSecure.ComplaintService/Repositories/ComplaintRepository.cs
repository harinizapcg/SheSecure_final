using Microsoft.EntityFrameworkCore;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Repositories
{
    public class ComplaintRepository : IComplaintRepository
    {
        private readonly ComplaintDbContext _context;

        public ComplaintRepository(ComplaintDbContext context)
        {
            _context = context;
        }

        public async Task<Complaint> CreateComplaintAsync(Complaint complaint)
        {
            _context.Complaints.Add(complaint);

            await _context.SaveChangesAsync();

            return complaint;
        }

        public async Task<List<Complaint>> GetAllComplaintsAsync()
        {
            return await _context.Complaints.ToListAsync();
        }

        public async Task<Complaint> GetComplaintByIdAsync(int id)
        {
            return await _context.Complaints
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateComplaintAsync(Complaint complaint)
        {
            _context.Complaints.Update(complaint);

            await _context.SaveChangesAsync();
        }

        public async Task<List<Complaint>> GetComplaintsByEmployeeAsync(
            string employeeId)
        {
            return await _context.Complaints
                .Where(x => x.EmployeeId == employeeId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}