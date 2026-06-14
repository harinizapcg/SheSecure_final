using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintController : ControllerBase
    {
        private readonly IComplaintService _service;

        public ComplaintController(IComplaintService service)
        {
            _service = service;
        }

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateComplaint(
            [FromBody] CreateComplaintDTO dto)
        {
            var employeeId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value 
                ?? User.FindFirst("sub")?.Value 
                ?? "1";

            var result = await _service.CreateComplaintAsync(
                dto,
                employeeId);

            return Ok(result);
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAllComplaints()
        {
            var result = await _service.GetAllComplaintsAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetComplaintById(int id)
        {
            var result = await _service.GetComplaintByIdAsync(id);

            return Ok(result);
        }

        [HttpPut("status")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] UpdateComplaintStatusDTO dto)
        {
            await _service.UpdateComplaintStatusAsync(dto);

            return Ok("Complaint status updated");
        }

        [HttpPut("assign")]
        [Authorize(Roles = "HR,Admin")]
        public async Task<IActionResult> AssignComplaint(
            [FromBody] AssignComplaintDTO dto)
        {
            await _service.AssignComplaintAsync(dto);

            return Ok("Complaint assigned");
        }

        /// <summary>
        /// Returns complaints belonging to a specific employee.
        /// Employees call this with their own ID; admins can call with any ID.
        /// </summary>
        [HttpGet("by-employee/{employeeId}")]
        [Authorize]
        public async Task<IActionResult> GetComplaintsByEmployee(
            string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
                return BadRequest("employeeId is required.");

            var result = await _service
                .GetComplaintsByEmployeeAsync(employeeId);

            return Ok(result);
        }
    }
}