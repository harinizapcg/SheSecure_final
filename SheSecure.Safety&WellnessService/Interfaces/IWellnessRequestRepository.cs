using SheSecure.WellnessSafetyService.Entities;

namespace SheSecure.WellnessSafetyService.Interfaces
{
    public interface IWellnessRequestRepository
    {
        Task<WellnessRequest>
            CreateRequestAsync(
                WellnessRequest request);

        Task<List<WellnessRequest>>
            GetAllRequestsAsync();


        //new
        Task<List<WellnessRequest>> GetByEmployeeIdAsync(string employeeId);
        Task<WellnessRequest?>
            GetByIdAsync(int id);

        Task UpdateRequestAsync(
            WellnessRequest request);
    }
}