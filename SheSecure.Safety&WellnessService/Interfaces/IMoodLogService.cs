using SheSecure.Safety_WellnessService.DTOs;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface IMoodLogService
    {
        Task<MoodLogResponseDTO> AddLogAsync(
            CreateMoodLogDTO dto);

        Task<List<MoodLogResponseDTO>> GetLogsByEmployeeAsync(
            string employeeId);

        Task<BurnoutScoreDTO> GetBurnoutScoreAsync(
            string employeeId);
    }
}