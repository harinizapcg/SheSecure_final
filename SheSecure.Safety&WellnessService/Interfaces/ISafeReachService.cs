using SheSecure.Safety_WellnessService.DTOs;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface ISafeReachService
    {
        Task CreateAsync(CreateSafeReachDTO dto);

        Task AcknowledgeAsync(AcknowledgeSafeReachDTO dto);

        Task<object> GetAllAsync();

        Task<object> GetByIdAsync(int id);

        Task EscalateAsync(int id);

        Task<object> GetByEmployeeAsync(string employeeId);
    }
}