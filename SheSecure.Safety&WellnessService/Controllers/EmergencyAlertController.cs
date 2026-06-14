using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SheSecure.Safety_WellnessService.DTOs.Requests;
using SheSecure.Safety_WellnessService.Interfaces;

namespace SheSecure.Safety_WellnessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmergencyAlertController : ControllerBase
    {
        private readonly IEmergencyAlertService _service;

        public EmergencyAlertController(
            IEmergencyAlertService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create(
            CreateEmergencyAlertDTO dto)
        {
            var result =
                await _service.CreateAlertAsync(dto);

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