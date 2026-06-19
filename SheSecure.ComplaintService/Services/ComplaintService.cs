//using SheSecure.ComplaintService.DTOs.Requests;
//using SheSecure.ComplaintService.DTOs.Responses;
//using SheSecure.ComplaintService.Entities;
//using SheSecure.ComplaintService.Interfaces;

//namespace SheSecure.ComplaintService.Services
//{
//    public class ComplaintService : IComplaintService
//    {
//        private readonly IComplaintRepository _repository;
//        //private readonly IComplaintStatusHistoryRepository _historyRepository;
//        public ComplaintService(IComplaintRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task<ComplaintResponseDTO> CreateComplaintAsync(
//            CreateComplaintDTO dto,
//            string employeeId)
//        {
//            var complaint = new Complaint
//            {
//                EmployeeId = employeeId,
//                Category = dto.Category,
//                Subject = dto.Subject,
//                Description = dto.Description,
//                Priority = dto.Priority,
//                IsAnonymous = dto.IsAnonymous,
//                Status = "Submitted",
//                ComplaintNumber = GenerateComplaintNumber()
//            };

//            var createdComplaint =
//                await _repository.CreateComplaintAsync(complaint);

//            return new ComplaintResponseDTO
//            {
//                Id = createdComplaint.Id,
//                ComplaintNumber = createdComplaint.ComplaintNumber,
//                Subject = createdComplaint.Subject,
//                Status = createdComplaint.Status,
//                CreatedAt = createdComplaint.CreatedAt
//            };
//        }

//        public async Task<List<ComplaintResponseDTO>> GetAllComplaintsAsync()
//        {
//            var complaints = await _repository.GetAllComplaintsAsync();

//            return complaints.Select(x => new ComplaintResponseDTO
//            {
//                Id = x.Id,
//                ComplaintNumber = x.ComplaintNumber,
//                Subject = x.Subject,
//                Status = x.Status,
//                CreatedAt = x.CreatedAt
//            }).ToList();
//        }

//        public async Task<ComplaintResponseDTO> GetComplaintByIdAsync(int id)
//        {
//            var complaint = await _repository.GetComplaintByIdAsync(id);

//            if (complaint == null)
//                throw new Exception("Complaint not found");

//            return new ComplaintResponseDTO
//            {
//                Id = complaint.Id,
//                ComplaintNumber = complaint.ComplaintNumber,
//                Subject = complaint.Subject,
//                Status = complaint.Status,
//                CreatedAt = complaint.CreatedAt
//            };
//        }

//        public async Task UpdateComplaintStatusAsync(
//            UpdateComplaintStatusDTO dto)
//        {
//            var complaint =
//                await _repository.GetComplaintByIdAsync(dto.ComplaintId);

//            if (complaint == null)
//                throw new Exception("Complaint not found");

//            complaint.Status = dto.Status;
//            complaint.ResolutionNotes = dto.ResolutionNotes;
//            complaint.UpdatedAt = DateTime.UtcNow;

//            await _repository.UpdateComplaintAsync(complaint);
//        }

//        public async Task AssignComplaintAsync(AssignComplaintDTO dto)
//        {
//            var complaint =
//                await _repository.GetComplaintByIdAsync(dto.ComplaintId);

//            if (complaint == null)
//                throw new Exception("Complaint not found");

//            complaint.AssignedTo = dto.AssignedTo;

//            await _repository.UpdateComplaintAsync(complaint);
//        }

//        private string GenerateComplaintNumber()
//        {
//            return $"CMP-{DateTime.UtcNow.Ticks}";
//        }
//    }
//}
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.DTOs.Responses;
using SheSecure.ComplaintService.Entities;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Services
{
    public class ComplaintService : IComplaintService
    {
        private readonly IComplaintRepository _repository;
        private readonly IComplaintStatusHistoryRepository _historyRepository;
        private readonly HttpClient _http;
        private readonly ILogger<ComplaintService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ComplaintService(
            IComplaintRepository repository,
            IComplaintStatusHistoryRepository historyRepository,
            IHttpClientFactory httpFactory,
            ILogger<ComplaintService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _historyRepository = historyRepository;
            _http = httpFactory.CreateClient("NotificationService");
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private (string employeeId, string email, string role) GetUserContext()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null)
            {
                return ("1", "", "Employee");
            }

            var employeeId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value
                ?? user.FindFirst("nameid")?.Value
                ?? "1";

            var email = user.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                ?? user.FindFirst("name")?.Value
                ?? "";

            var role = user.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                ?? user.FindFirst("role")?.Value
                ?? "Employee";

            return (employeeId, email, role);
        }

        public async Task<ComplaintResponseDTO> CreateComplaintAsync(
            CreateComplaintDTO dto,
            string employeeId)
        {
            var complaint = new Complaint
            {
                EmployeeId = employeeId,
                Category = dto.Category,
                Subject = dto.Subject,
                Description = dto.Description,
                Priority = dto.Priority,
                IsAnonymous = dto.IsAnonymous,
                Status = "Submitted",
                ComplaintNumber = GenerateComplaintNumber()
            };

            var created =
                await _repository.CreateComplaintAsync(complaint);

            var empIdInt = int.TryParse(employeeId, out var parsedId) ? parsedId : 1;
            await _historyRepository.AddHistoryAsync(new ComplaintStatusHistory
            {
                ComplaintId = created.Id,
                OldStatus = "-",
                NewStatus = "Submitted",
                ChangedBy = empIdInt,
                Remarks = "Complaint submitted.",
                ChangedAt = DateTime.UtcNow
            });

            // Don't notify if anonymous — employee chose not
            // to be identified
            if (!dto.IsAnonymous)
            {
                await SendNotificationAsync(
                    employeeId,
                    "Complaint Submitted",
                    $"Your complaint ({created.ComplaintNumber}) has been submitted successfully.",
                    "COMPLAINT_SUBMITTED");
            }

            return new ComplaintResponseDTO
            {
                Id = created.Id,
                ComplaintNumber = created.ComplaintNumber,
                Subject = created.Subject,
                Category = created.Category,
                Priority = created.Priority,
                AssignedTo = created.AssignedTo,
                Status = created.Status,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task<List<ComplaintResponseDTO>>
            GetAllComplaintsAsync()
        {
            var (userId, email, role) = GetUserContext();
            List<Complaint> complaints;

            if (role == "HR" || role == "Admin" || role == "Manager")
            {
                complaints = await _repository.GetAllComplaintsAsync();
            }
            else
            {
                complaints = await _repository.GetComplaintsByEmployeeAsync(userId);
            }

            return complaints.Select(x =>
                new ComplaintResponseDTO
                {
                    Id = x.Id,
                    ComplaintNumber = x.ComplaintNumber,
                    Subject = x.Subject,
                    Category = x.Category,
                    Priority = x.Priority,
                    AssignedTo = x.AssignedTo,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task<ComplaintResponseDTO>
            GetComplaintByIdAsync(int id)
        {
            var complaint =
                await _repository.GetComplaintByIdAsync(id);

            if (complaint == null)
                throw new Exception("Complaint not found");

            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && complaint.EmployeeId != userId && complaint.EmployeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this complaint.");
            }

            return new ComplaintResponseDTO
            {
                Id = complaint.Id,
                ComplaintNumber = complaint.ComplaintNumber,
                Subject = complaint.Subject,
                Category = complaint.Category,
                Priority = complaint.Priority,
                AssignedTo = complaint.AssignedTo,
                Status = complaint.Status,
                CreatedAt = complaint.CreatedAt
            };
        }

        public async Task UpdateComplaintStatusAsync(
            UpdateComplaintStatusDTO dto)
        {
            var complaint =
                await _repository.GetComplaintByIdAsync(
                    dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found");

            var oldStatus = complaint.Status;
            complaint.Status = dto.Status;
            complaint.ResolutionNotes = dto.ResolutionNotes;
            complaint.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateComplaintAsync(complaint);

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdStr = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1";
            var userId = int.TryParse(userIdStr, out var uid) ? uid : 1;

            var history = new ComplaintStatusHistory
            {
                ComplaintId = complaint.Id,
                OldStatus = oldStatus,
                NewStatus = dto.Status,
                ChangedBy = userId,
                Remarks = dto.ResolutionNotes ?? "",
                ChangedAt = DateTime.UtcNow
            };
            await _historyRepository.AddHistoryAsync(history);

            if (!complaint.IsAnonymous)
            {
                await SendNotificationAsync(
                    complaint.EmployeeId,
                    "Complaint Status Updated",
                    $"Your complaint ({complaint.ComplaintNumber}) status has been updated to: {dto.Status}.",
                    "COMPLAINT_STATUS_UPDATED");
            }
        }

        public async Task AssignComplaintAsync(
            AssignComplaintDTO dto)
        {
            var complaint =
                await _repository.GetComplaintByIdAsync(
                    dto.ComplaintId);

            if (complaint == null)
                throw new Exception("Complaint not found");

            var oldStatus = complaint.Status;
            complaint.AssignedTo = dto.AssignedTo;

            if (complaint.Status == "Submitted" || complaint.Status == "Under Review")
            {
                complaint.Status = "Under Investigation";
            }

            await _repository.UpdateComplaintAsync(complaint);

            var user = _httpContextAccessor.HttpContext?.User;
            var userIdStr = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "1";
            var userId = int.TryParse(userIdStr, out var uid) ? uid : 1;

            await _historyRepository.AddHistoryAsync(new ComplaintStatusHistory
            {
                ComplaintId = complaint.Id,
                OldStatus = oldStatus,
                NewStatus = complaint.Status,
                ChangedBy = userId,
                Remarks = $"Assigned to investigator: {dto.AssignedTo}",
                ChangedAt = DateTime.UtcNow
            });

            if (!complaint.IsAnonymous)
            {
                await SendNotificationAsync(
                    complaint.EmployeeId,
                    "Complaint Assigned",
                    $"Your complaint ({complaint.ComplaintNumber}) has been assigned and is being reviewed.",
                    "COMPLAINT_ASSIGNED");
            }
        }

        private string GenerateComplaintNumber() =>
            $"CMP-{DateTime.UtcNow.Ticks}";

        public async Task<List<ComplaintResponseDTO>>
            GetComplaintsByEmployeeAsync(string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && employeeId != userId && employeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these complaints.");
            }

            var complaints =
                await _repository.GetComplaintsByEmployeeAsync(
                    employeeId);

            return complaints.Select(x =>
                new ComplaintResponseDTO
                {
                    Id = x.Id,
                    ComplaintNumber = x.ComplaintNumber,
                    Subject = x.Subject,
                    Category = x.Category,
                    Priority = x.Priority,
                    AssignedTo = x.AssignedTo,
                    Status = x.Status,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        private async Task SendNotificationAsync(
            string employeeId,
            string title,
            string message,
            string type)
        {
            try
            {
                var payload = JsonSerializer.Serialize(
                    new { employeeId, title, message, type },
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy =
                            JsonNamingPolicy.CamelCase
                    });

                await _http.PostAsync(
                    "api/Notification/create",
                    new StringContent(
                        payload,
                        Encoding.UTF8,
                        "application/json"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send notification [{Type}] " +
                    "for employee {EmployeeId}", type, employeeId);
            }
        }
    }
}