using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SheSecure.Safety_WellnessService.Data;
using SheSecure.Safety_WellnessService.Entities;

namespace SheSecure.Safety_WellnessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private readonly WellnessDbContext _context;

        public AuditController(WellnessDbContext context)
        {
            _context = context;
        }

        // GET: api/Audit
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuditLog>>> GetAuditLogs()
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(100)
                .ToListAsync();
        }

        // POST: api/Audit
        [HttpPost]
        public async Task<ActionResult<AuditLog>> PostAuditLog([FromBody] AuditLog log)
        {
            log.Timestamp = DateTime.UtcNow;
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAuditLogs", new { id = log.Id }, log);
        }
    }
}
