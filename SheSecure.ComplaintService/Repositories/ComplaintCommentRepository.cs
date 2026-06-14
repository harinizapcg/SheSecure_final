using Microsoft.EntityFrameworkCore;
using SheSecure.ComplaintService.Data;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Repositories
{
    public class ComplaintCommentRepository
        : IComplaintCommentRepository
    {
        private readonly ComplaintDbContext _context;

        public ComplaintCommentRepository(
            ComplaintDbContext context)
        {
            _context = context;
        }

        public async Task<ComplaintComment> AddCommentAsync(
            ComplaintComment comment)
        {
            _context.ComplaintComments.Add(comment);

            await _context.SaveChangesAsync();

            return comment;
        }

        public async Task<List<ComplaintComment>>
            GetCommentsByComplaintIdAsync(int complaintId)
        {
            return await _context.ComplaintComments
                .Where(x => x.ComplaintId == complaintId)
                .OrderBy(x => x.CreatedAt)
                .ToListAsync();
        }
    }
}