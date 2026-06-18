using SheSecure.ComplaintService.Data;
using Microsoft.EntityFrameworkCore;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Repositories
{
    public class ComplaintFileRepository : IComplaintFileRepository
    {
        private readonly ComplaintDbContext _context;

        public ComplaintFileRepository(ComplaintDbContext context)
        {
            _context = context;
        }

        public async Task<ComplaintFile> AddFileAsync(ComplaintFile file)
        {
            _context.ComplaintFiles.Add(file);

            await _context.SaveChangesAsync();

            return file;
        }

        public async Task<List<ComplaintFile>> GetFilesByComplaintIdAsync(int complaintId)
        {
            return await _context.ComplaintFiles
                .Where(x => x.ComplaintId == complaintId)
                .ToListAsync();
        }
    }
}