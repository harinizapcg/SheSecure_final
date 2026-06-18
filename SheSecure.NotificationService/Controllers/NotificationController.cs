using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheSecure.NotificationService.DTOs.Requests;
using SheSecure.NotificationService.Interfaces;
using SheSecure.NotificationService.Services;

namespace SheSecure.NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(
            INotificationService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            CreateNotificationDTO dto)
        {
            var result =
                await _service.CreateNotificationAsync(dto);

            return Ok(result);
        }
        //email testing endpoint for development purposes
        [HttpGet("test-email")]
        public async Task<IActionResult> TestEmail()
        {
            var emailService = new EmailService();

            await emailService.SendEmailAsync(
    "harinibanda27@gmail.com",
    "SheSecure Test",
    "<h2>Email Service Working Successfully!</h2>");

            return Ok("Mail Sent Successfully");
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            return Ok(
                await _service.GetAllNotificationsAsync());
        }

        [HttpGet("employee/{employeeId}")]
        [Authorize]
        public async Task<IActionResult>
            GetEmployeeNotifications(
                string employeeId)
        {
            return Ok(
                await _service
                    .GetEmployeeNotificationsAsync(
                        employeeId));
        }

        /// <summary>
        /// Returns notifications belonging to a specific employee.
        /// Employees call this with their own ID; admins can call with any ID.
        /// </summary>
        [HttpGet("by-employee/{employeeId}")]
        [Authorize]
        public async Task<IActionResult>
            GetNotificationsByEmployee(
                string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest("employeeId is required.");

            return Ok(
                await _service
                    .GetEmployeeNotificationsAsync(
                        employeeId));
        }

        [HttpPut("read/{id}")]
        [Authorize]
        public async Task<IActionResult>
            MarkAsRead(int id)
        {
            await _service.MarkAsReadAsync(id);

            return Ok(new
            {
                Message =
                    "Notification marked as read"
            });
        }
    }
}