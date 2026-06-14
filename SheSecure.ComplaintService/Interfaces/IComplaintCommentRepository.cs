using SheSecure.ComplaintService.Entities;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintCommentRepository
    {
        Task<ComplaintComment> AddCommentAsync(
            ComplaintComment comment);

        Task<List<ComplaintComment>> GetCommentsByComplaintIdAsync(
            int complaintId);
    }
}