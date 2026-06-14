using SheSecure.Safety_WellnessService.Models;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface IMoodLogRepository
    {
        Task<MoodLog> AddLogAsync(
            MoodLog log);

        Task<List<MoodLog>> GetByEmployeeIdAsync(
            string employeeId);

        Task<List<MoodLog>> GetRecentByEmployeeIdAsync(
            string employeeId, int days);
    }
}