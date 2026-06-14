using Microsoft.AspNetCore.Mvc;
using SheSecure.ComplaintService.Interfaces;

namespace SheSecure.ComplaintService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ComplaintFilesController : ControllerBase
    {
        private readonly IComplaintFileService _service;

        public ComplaintFilesController(
            IComplaintFileService service)
        {
            _service = service;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(
            int complaintId,
            IFormFile file)
        {
            var result =
                await _service.UploadFileAsync(
                    complaintId,
                    file);

            return Ok(result);
        }
    }
}