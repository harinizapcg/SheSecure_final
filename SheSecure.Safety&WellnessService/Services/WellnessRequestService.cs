//using SheSecure.WellnessSafetyService.DTOs.Requests;
//using SheSecure.WellnessSafetyService.DTOs.Responses;
//using SheSecure.WellnessSafetyService.Entities;
//using SheSecure.WellnessSafetyService.Interfaces;

//namespace SheSecure.WellnessSafetyService.Services
//{
//    public class WellnessRequestService
//        : IWellnessRequestService
//    {
//        private readonly
//            IWellnessRequestRepository _repository;

//        public WellnessRequestService(
//            IWellnessRequestRepository repository)
//        {
//            _repository = repository;
//        }

//        public async Task<WellnessRequestResponseDTO>
//            CreateRequestAsync(
//                CreateWellnessRequestDTO dto)
//        {
//            var request = new WellnessRequest
//            {
//                EmployeeId = dto.EmployeeId,
//                RequestType = dto.RequestType,
//                Description = dto.Description,
//                Priority = dto.Priority
//            };

//            var savedRequest =
//                await _repository.CreateRequestAsync(
//                    request);

//            return new WellnessRequestResponseDTO
//            {
//                Id = savedRequest.Id,
//                EmployeeId = savedRequest.EmployeeId,
//                RequestType = savedRequest.RequestType,
//                Description = savedRequest.Description,
//                Priority = savedRequest.Priority,
//                Status = savedRequest.Status,
//                AssignedTo = savedRequest.AssignedTo,
//                CreatedAt = savedRequest.CreatedAt
//            };
//        }

//        public async Task<
//            List<WellnessRequestResponseDTO>>
//            GetAllRequestsAsync()
//        {
//            var requests =
//                await _repository.GetAllRequestsAsync();

//            return requests.Select(x =>
//                new WellnessRequestResponseDTO
//                {
//                    Id = x.Id,
//                    EmployeeId = x.EmployeeId,
//                    RequestType = x.RequestType,
//                    Description = x.Description,
//                    Priority = x.Priority,
//                    Status = x.Status,
//                    AssignedTo = x.AssignedTo,
//                    CreatedAt = x.CreatedAt
//                }).ToList();
//        }

//        public async Task<
//            WellnessRequestResponseDTO?>
//            GetByIdAsync(int id)
//        {
//            var request =
//                await _repository.GetByIdAsync(id);

//            if (request == null)
//            {
//                return null;
//            }

//            return new WellnessRequestResponseDTO
//            {
//                Id = request.Id,
//                EmployeeId = request.EmployeeId,
//                RequestType = request.RequestType,
//                Description = request.Description,
//                Priority = request.Priority,
//                Status = request.Status,
//                AssignedTo = request.AssignedTo,
//                CreatedAt = request.CreatedAt
//            };
//        }

//        public async Task UpdateStatusAsync(
//            UpdateWellnessRequestStatusDTO dto)
//        {
//            var request =
//                await _repository.GetByIdAsync(
//                    dto.RequestId);

//            if (request == null)
//            {
//                throw new Exception(
//                    "Wellness request not found");
//            }

//            request.Status = dto.Status;
//            request.AssignedTo = dto.AssignedTo;

//            await _repository.UpdateRequestAsync(
//                request);
//        }
//    }
//}

using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using SheSecure.WellnessSafetyService.DTOs.Requests;
using SheSecure.WellnessSafetyService.DTOs.Responses;
using SheSecure.WellnessSafetyService.Entities;
using SheSecure.WellnessSafetyService.Interfaces;

namespace SheSecure.WellnessSafetyService.Services
{
    public class WellnessRequestService
        : IWellnessRequestService
    {
        private readonly IWellnessRequestRepository _repository;
        private readonly HttpClient _http;
        private readonly ILogger<WellnessRequestService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WellnessRequestService(
            IWellnessRequestRepository repository,
            IHttpClientFactory httpFactory,
            ILogger<WellnessRequestService> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
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

        public async Task<WellnessRequestResponseDTO>
            CreateRequestAsync(CreateWellnessRequestDTO dto)
        {
            var request = new WellnessRequest
            {
                EmployeeId = dto.EmployeeId,
                RequestType = dto.RequestType,
                Description = dto.Description,
                Priority = dto.Priority,
                RequestDate = dto.RequestDate
            };

            var saved =
                await _repository.CreateRequestAsync(request);

            await SendNotificationAsync(
                dto.EmployeeId.ToString(),
                "Wellness Request Submitted",
                "Your wellness request has been submitted successfully.",
                "WELLNESS_REQUESTED");

            return new WellnessRequestResponseDTO
            {
                Id = saved.Id,
                EmployeeId = saved.EmployeeId,
                RequestType = saved.RequestType,
                Description = saved.Description,
                Priority = saved.Priority,
                Status = saved.Status,
                AssignedTo = saved.AssignedTo,
                RequestDate = saved.RequestDate,
                CreatedAt = saved.CreatedAt
            };
        }

        public async Task<List<WellnessRequestResponseDTO>>
            GetAllRequestsAsync()
        {
            var (userId, email, role) = GetUserContext();
            List<WellnessRequest> requests;

            if (role == "HR" || role == "Admin" || role == "Manager")
            {
                requests = await _repository.GetAllRequestsAsync();
            }
            else
            {
                requests = await _repository.GetByEmployeeIdAsync(userId);
            }

            return requests.Select(x =>
                new WellnessRequestResponseDTO
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    RequestType = x.RequestType,
                    Description = x.Description,
                    Priority = x.Priority,
                    Status = x.Status,
                    AssignedTo = x.AssignedTo,
                    RequestDate = x.RequestDate,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task<WellnessRequestResponseDTO?>
            GetByIdAsync(int id)
        {
            var request = await _repository.GetByIdAsync(id);

            if (request == null)
                return null;

            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && request.EmployeeId != userId && request.EmployeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this request.");
            }

            return new WellnessRequestResponseDTO
            {
                Id = request.Id,
                EmployeeId = request.EmployeeId,
                RequestType = request.RequestType,
                Description = request.Description,
                Priority = request.Priority,
                Status = request.Status,
                AssignedTo = request.AssignedTo,
                RequestDate = request.RequestDate,
                CreatedAt = request.CreatedAt
            };
        }

        public async Task UpdateStatusAsync(
            UpdateWellnessRequestStatusDTO dto)
        {
            var request =
                await _repository.GetByIdAsync(dto.RequestId);

            if (request == null)
                throw new Exception(
                    "Wellness request not found");

            request.Status = dto.Status;
            request.AssignedTo = dto.AssignedTo;

            await _repository.UpdateRequestAsync(request);

            await SendNotificationAsync(
                request.EmployeeId.ToString(),
                "Wellness Request Updated",
                $"Your wellness request status has been updated to: {dto.Status}.",
                "WELLNESS_STATUS_UPDATED");
        }

        // existing method kept for compatibility, updated to string
        public async Task<List<WellnessRequestResponseDTO>> GetMyRequestsAsync(string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && employeeId != userId && employeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these requests.");
            }

            var requests = await _repository.GetByEmployeeIdAsync(employeeId);

            return requests.Select(r => new WellnessRequestResponseDTO
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                RequestType = r.RequestType,
                Description = r.Description,
                Priority = r.Priority,
                Status = r.Status,
                AssignedTo = r.AssignedTo,
                RequestDate = r.RequestDate,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        // new string-based method aligned with by-employee route pattern
        public async Task<List<WellnessRequestResponseDTO>>
            GetByEmployeeAsync(string employeeId)
        {
            return await GetMyRequestsAsync(employeeId);
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