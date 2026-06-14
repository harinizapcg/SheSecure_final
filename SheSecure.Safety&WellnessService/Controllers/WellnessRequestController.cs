using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SheSecure.WellnessSafetyService.DTOs.Requests;
using SheSecure.WellnessSafetyService.Interfaces;
using System.Security.Claims;

namespace SheSecure.WellnessSafetyService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WellnessRequestController
        : ControllerBase
    {
        private readonly
            IWellnessRequestService _service;

        public WellnessRequestController(
            IWellnessRequestService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        public async Task<IActionResult>
            CreateRequest(
                [FromBody]
                CreateWellnessRequestDTO dto)
        {
            var result =
                await _service.CreateRequestAsync(dto);

            return Ok(result);
        }

        [HttpGet("all")]
        public async Task<IActionResult>
            GetAllRequests()
        {
            var result =
                await _service.GetAllRequestsAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult>
            GetById(int id)
        {
            var result =
                await _service.GetByIdAsync(id);

            if (result == null)
            {
                return NotFound(
                    "Wellness request not found");
            }

            return Ok(result);
        }
        [HttpGet("my")]
        public async Task<IActionResult> GetMyRequests([FromQuery] string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest("Invalid employee ID.");

            var result = await _service.GetMyRequestsAsync(employeeId);

            return Ok(result);
        }

        /// <summary>
        /// Returns wellness requests belonging to a specific employee.
        /// Employees call this with their own ID; admins can call with any ID.
        /// </summary>
        [HttpGet("by-employee/{employeeId}")]
        public async Task<IActionResult>
            GetByEmployee(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest("employeeId is required.");

            try
            {
                var result =
                    await _service.GetByEmployeeAsync(employeeId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult>
            UpdateStatus(
                [FromBody]
                UpdateWellnessRequestStatusDTO dto)
        {
            await _service.UpdateStatusAsync(dto);

            return Ok(
                "Wellness request updated successfully");
        }
    }
}