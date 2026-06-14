using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;

namespace SheSecure.ComplaintService.Interfaces
{
    public interface IComplaintService
    {
        Task<ComplaintResponseDTO> CreateComplaintAsync(
            CreateComplaintDTO dto,
            string employeeId);

        Task<List<ComplaintResponseDTO>> GetAllComplaintsAsync();

        Task<ComplaintResponseDTO> GetComplaintByIdAsync(int id);

        Task UpdateComplaintStatusAsync(UpdateComplaintStatusDTO dto);

        Task AssignComplaintAsync(AssignComplaintDTO dto);

        Task<List<ComplaintResponseDTO>> GetComplaintsByEmployeeAsync(
            string employeeId);
    }
}