using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SheSecure.Safety_WellnessService.DTOs;
using SheSecure.Safety_WellnessService.Interfaces;

namespace SheSecure.Safety_WellnessService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MoodLogController : ControllerBase
    {
        private readonly IMoodLogService _service;

        public MoodLogController(IMoodLogService service)
        {
            _service = service;
        }

        // POST /api/MoodLog/add
        [HttpPost("add")]
        public async Task<IActionResult> AddLog([FromBody] CreateMoodLogDTO dto)
        {
            var result = await _service.AddLogAsync(dto);

            return Ok(result);
        }

        // GET /api/MoodLog/{employeeId}
        [HttpGet("{employeeId}")]
        public async Task<IActionResult> GetLogs(string employeeId)
        {
            var result = await _service.GetLogsByEmployeeAsync(employeeId);

            return Ok(result);
        }

        // GET /api/MoodLog/burnout-score/{employeeId}
        [HttpGet("burnout-score/{employeeId}")]
        public async Task<IActionResult> GetBurnoutScore(string employeeId)
        {
            var result = await _service.GetBurnoutScoreAsync(employeeId);

            return Ok(result);
        }
    }
}