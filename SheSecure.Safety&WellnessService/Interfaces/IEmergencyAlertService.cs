using SheSecure.Safety_WellnessService.DTOs.Requests;
using SheSecure.Safety_WellnessService.Entities;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface IEmergencyAlertService
    {
        Task<EmergencyAlert> CreateAlertAsync(
            CreateEmergencyAlertDTO dto);

        Task<List<EmergencyAlert>> GetAllAlertsAsync();

        Task<EmergencyAlert?> GetAlertByIdAsync(
            int id);

        Task ResolveAlertAsync(
            ResolveEmergencyAlertDTO dto);
    }
}