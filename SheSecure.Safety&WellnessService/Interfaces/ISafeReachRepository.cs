using SheSecure.Safety_WellnessService.Entities;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface ISafeReachRepository
    {
        Task<SafeReachCheck> CreateAsync(SafeReachCheck check);

        Task<List<SafeReachCheck>> GetAllAsync();

        Task<SafeReachCheck?> GetByIdAsync(int id);

        Task UpdateAsync(SafeReachCheck check);

        Task<List<SafeReachCheck>> GetByEmployeeAsync(string employeeId);
    }
}