using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Services
{
    public class ComplaintStatusHistoryService
        : IComplaintStatusHistoryService
    {
        private readonly
            IComplaintStatusHistoryRepository
            _repository;

        public ComplaintStatusHistoryService(
            IComplaintStatusHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<
            ComplaintStatusHistoryResponseDTO>
            AddHistoryAsync(
                AddComplaintStatusHistoryDTO dto)
        {
            var history = new ComplaintStatusHistory
            {
                ComplaintId = dto.ComplaintId,
                OldStatus = dto.OldStatus,
                NewStatus = dto.NewStatus,
                ChangedBy = dto.ChangedBy,
                Remarks = dto.Remarks
            };

            var saved =
                await _repository.AddHistoryAsync(
                    history);

            return new
                ComplaintStatusHistoryResponseDTO
            {
                Id = saved.Id,
                ComplaintId = saved.ComplaintId,
                OldStatus = saved.OldStatus,
                NewStatus = saved.NewStatus,
                ChangedBy = saved.ChangedBy,
                Remarks = saved.Remarks,
                ChangedAt = saved.ChangedAt
            };
        }

        public async Task<
            List<ComplaintStatusHistoryResponseDTO>>
            GetHistoryByComplaintIdAsync(
                int complaintId)
        {
            var history =
                await _repository
                    .GetHistoryByComplaintIdAsync(
                        complaintId);

            return history.Select(x =>
                new ComplaintStatusHistoryResponseDTO
                {
                    Id = x.Id,
                    ComplaintId = x.ComplaintId,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    ChangedBy = x.ChangedBy,
                    Remarks = x.Remarks,
                    ChangedAt = x.ChangedAt
                }).ToList();
        }
    }
}