using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintCommentService
    {
        Task<ComplaintCommentResponseDTO> AddCommentAsync(
            AddComplaintCommentDTO dto);

        Task<List<ComplaintCommentResponseDTO>>
            GetCommentsByComplaintIdAsync(int complaintId);
    }
}