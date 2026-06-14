using Microsoft.AspNetCore.Mvc;
using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintCommentsController : ControllerBase
    {
        private readonly IComplaintCommentService _service;

        public ComplaintCommentsController(
            IComplaintCommentService service)
        {
            _service = service;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddComment(
            [FromBody] AddComplaintCommentDTO dto)
        {
            var result =
                await _service.AddCommentAsync(dto);

            return Ok(result);
        }

        [HttpGet("{complaintId}")]
        public async Task<IActionResult> GetComments(
            int complaintId)
        {
            var result =
                await _service
                    .GetCommentsByComplaintIdAsync(
                        complaintId);

            return Ok(result);
        }
    }
}