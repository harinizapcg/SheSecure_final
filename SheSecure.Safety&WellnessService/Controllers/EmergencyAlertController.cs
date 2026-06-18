using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheSecure.Safety_WellnessService.DTOs.Requests;
using SheSecure.Safety_WellnessService.Interfaces;
using SheSecure.Safety_WellnessService.Services;

namespace SheSecure.Safety_WellnessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmergencyAlertController : ControllerBase
    {
        private readonly IEmergencyAlertService _service;


        private readonly IEmailService _emailService;

        public EmergencyAlertController(
            IEmergencyAlertService service,
            IEmailService emailService)
        {
            _service = service;
            _emailService = emailService;
        }

        [HttpGet("test-di")]
        public IActionResult TestDI()
        {
            return Ok("EmailService injected successfully");
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            CreateEmergencyAlertDTO dto)
        {
            var result =
                await _service.CreateAlertAsync(dto);

            try
            {
                var body = $"<h2 style='color:red;'>🚨 EMERGENCY SOS TRIGGERED 🚨</h2>" +
                           $"<p><strong>Employee ID:</strong> {dto.EmployeeId}</p>" +
                           $"<p><strong>Location:</strong> {dto.Location}</p>" +
                           $"<p><strong>Description:</strong> {dto.Description}</p>" +
                           $"<p><strong>Severity:</strong> {dto.Severity}</p>" +
                           $"<p><strong>Time:</strong> {DateTime.UtcNow} UTC</p>" +
                           $"<hr/><p>Please respond immediately via the SheSecure Dashboard.</p>";

                await _emailService.SendEmailAsync("harinibanda27@gmail.com", "🚨 URGENT: SheSecure SOS Alert", body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send SOS email: {ex.Message}");
            }

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var result =
                await _service.GetAllAlertsAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result =
                await _service.GetAlertByIdAsync(id);

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPut("resolve")]
        public async Task<IActionResult> Resolve(
            ResolveEmergencyAlertDTO dto)
        {
            await _service.ResolveAlertAsync(dto);

            return Ok(new
            {
                Message = "Alert resolved successfully"
            });
        }
    }
}
