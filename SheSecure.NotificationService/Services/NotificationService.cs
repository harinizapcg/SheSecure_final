using Microsoft.AspNetCore.Http;
using SheSecure.NotificationService.DTOs.Requests;
using SheSecure.NotificationService.DTOs.Responses;
using SheSecure.NotificationService.Entities;
using SheSecure.NotificationService.Interfaces;

namespace SheSecure.NotificationService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public NotificationService(
            INotificationRepository repository,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
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

        public async Task<NotificationResponseDTO>
            CreateNotificationAsync(
                CreateNotificationDTO dto)
        {
            var notification = new Notification
            {
                EmployeeId = dto.EmployeeId,
                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type
            };

            var created =
                await _repository.CreateNotificationAsync(
                    notification);

            return new NotificationResponseDTO
            {
                Id = created.Id,
                EmployeeId = created.EmployeeId,
                Title = created.Title,
                Message = created.Message,
                Type = created.Type,
                IsRead = created.IsRead,
                CreatedAt = created.CreatedAt
            };
        }

        public async Task<List<NotificationResponseDTO>>
            GetAllNotificationsAsync()
        {
            var (userId, email, role) = GetUserContext();
            List<Notification> notifications;

            if (role == "HR" || role == "Admin" || role == "Manager")
            {
                notifications = await _repository.GetAllNotificationsAsync();
            }
            else
            {
                notifications = await _repository.GetEmployeeNotificationsAsync(userId);
            }

            return notifications.Select(x =>
                new NotificationResponseDTO
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task<List<NotificationResponseDTO>>
            GetEmployeeNotificationsAsync(
                string employeeId)
        {
            var (userId, email, role) = GetUserContext();
            if (role != "HR" && role != "Admin" && role != "Manager" && employeeId != userId && employeeId != email)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these notifications.");
            }

            var notifications =
                await _repository.GetEmployeeNotificationsAsync(
                    employeeId);

            return notifications.Select(x =>
                new NotificationResponseDTO
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    IsRead = x.IsRead,
                    CreatedAt = x.CreatedAt
                }).ToList();
        }

        public async Task MarkAsReadAsync(
            int notificationId)
        {
            var notification =
                await _repository.GetByIdAsync(
                    notificationId);

            if (notification == null)
                throw new Exception(
                    "Notification not found");

            notification.IsRead = true;

            await _repository.UpdateNotificationAsync(
                notification);
        }
    }
}