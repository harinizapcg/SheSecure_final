using SheSecure.ComplaintService.Entities;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintFileRepository
    {
        Task<ComplaintFile> AddFileAsync(ComplaintFile file);
    }
}