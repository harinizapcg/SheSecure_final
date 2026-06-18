using SheSecure.ComplaintService.Entities;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintFileRepository
    {
        Task<ComplaintFile> AddFileAsync(ComplaintFile file);
        Task<List<ComplaintFile>> GetFilesByComplaintIdAsync(int complaintId);
    }
}