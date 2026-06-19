using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Data;
using SheSecure.Safety_WellnessService.Entities;

namespace SheSecure.Safety_WellnessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemSettingsController : ControllerBase
    {
        private readonly WellnessDbContext _context;

        public SystemSettingsController(WellnessDbContext context)
        {
            _context = context;
        }

        // GET: api/SystemSettings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SystemSetting>>> GetSystemSettings()
        {
            return await _context.SystemSettings.ToListAsync();
        }

        // PUT: api/SystemSettings/5
        [HttpPut("{key}")]
        public async Task<IActionResult> PutSystemSetting(string key, [FromBody] string value)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting == null)
            {
                return NotFound($"Setting with key {key} not found.");
            }

            setting.SettingValue = value;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
