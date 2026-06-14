using Microsoft.AspNetCore.Mvc;
using SheSecure.ComplaintService.DTOs.Requests;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintStatusHistoryController
        : ControllerBase
    {
        private readonly
            IComplaintStatusHistoryService _service;

        public ComplaintStatusHistoryController(
            IComplaintStatusHistoryService service)
        {
            _service = service;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddHistory(
            [FromBody]
            AddComplaintStatusHistoryDTO dto)
        {
            var result =
                await _service.AddHistoryAsync(dto);

            return Ok(result);
        }

        [HttpGet("{complaintId}")]
        public async Task<IActionResult> GetHistory(
            int complaintId)
        {
            var result =
                await _service
                    .GetHistoryByComplaintIdAsync(
                        complaintId);

            return Ok(result);
        }
    }
}