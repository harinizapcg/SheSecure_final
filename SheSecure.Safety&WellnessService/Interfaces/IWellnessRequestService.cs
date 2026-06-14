using SheSecure.WellnessSafetyService.DTOs.Requests;
using SheSecure.WellnessSafetyService.DTOs.Responses;

namespace SheSecure.WellnessSafetyService.Interfaces
{
    public interface IWellnessRequestService
    {
        Task<WellnessRequestResponseDTO>
            CreateRequestAsync(
                CreateWellnessRequestDTO dto);

        Task<List<WellnessRequestResponseDTO>>
            GetAllRequestsAsync();

        Task<WellnessRequestResponseDTO?>
            GetByIdAsync(int id);
        // existing int-based method kept for backward compatibility
        Task<List<WellnessRequestResponseDTO>> GetMyRequestsAsync(string employeeId);

        // new string-based method aligned with by-employee route pattern
        Task<List<WellnessRequestResponseDTO>> GetByEmployeeAsync(string employeeId);

        Task UpdateStatusAsync(
            UpdateWellnessRequestStatusDTO dto);
    }
}