using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintStatusHistoryService
    {
        Task<ComplaintStatusHistoryResponseDTO>
            AddHistoryAsync(
                AddComplaintStatusHistoryDTO dto);

        Task<List<ComplaintStatusHistoryResponseDTO>>
            GetHistoryByComplaintIdAsync(
                int complaintId);
    }
}