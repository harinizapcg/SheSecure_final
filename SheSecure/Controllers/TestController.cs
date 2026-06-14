using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SheSecure.AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("public")]
        public IActionResult Public()
        {
            return Ok("Public API");
        }

        [Authorize]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok("Protected API");
        }

        [Authorize(Roles = "HR")]
        [HttpGet("hr-only")]
        public IActionResult HrOnly()
        {
            return Ok("HR Only API");
        }
    }
}