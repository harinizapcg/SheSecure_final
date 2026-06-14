using SheSecure.Safety_WellnessService.Entities;

namespace SheSecure.Safety_WellnessService.Interfaces
{
    public interface IEmergencyAlertRepository
    {
        Task<EmergencyAlert> CreateAlertAsync(EmergencyAlert alert);

        Task<List<EmergencyAlert>> GetAllAlertsAsync();

        Task<EmergencyAlert?> GetByIdAsync(int id);

        Task UpdateAlertAsync(EmergencyAlert alert);
    }
}