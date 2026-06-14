using SheSecure.ComplaintService.Data;
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
    }
}