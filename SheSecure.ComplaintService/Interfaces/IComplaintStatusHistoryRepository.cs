using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.DTOs.Requests;
namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintStatusHistoryRepository
    {
        Task<ComplaintStatusHistory>
            AddHistoryAsync(
                ComplaintStatusHistory history);

        Task<List<ComplaintStatusHistory>>
            GetHistoryByComplaintIdAsync(
                int complaintId);
    }
}