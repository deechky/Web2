using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TripService.Data;
using TripService.Dtos;
using TripService.Mapping;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/internal/trips")]
    public class InternalController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly IConfiguration _configuration;

        public InternalController(TripDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpGet("{id}/full")]
        public async Task<ActionResult<FullPlanDto>> GetFullPlan(Guid id, [FromHeader(Name = "X-Internal-Key")] string? internalKey)
        {
            var expectedKey = _configuration["Internal:ApiKey"];
            if (string.IsNullOrEmpty(expectedKey) || internalKey != expectedKey)
            {
                return Unauthorized();
            }

            var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == id);
            if (plan == null)
            {
                return NotFound();
            }

            var fullPlan = new FullPlanDto
            {
                Plan = plan.ToDto(),
                Destinacije = await _db.Destinacije.Where(d => d.PlanId == id).Select(d => d.ToDto()).ToListAsync(),
                Aktivnosti = await _db.Aktivnosti.Where(a => a.PlanId == id).Select(a => a.ToDto()).ToListAsync(),
                Troskovi = await _db.Troskovi.Where(t => t.PlanId == id).Select(t => t.ToDto()).ToListAsync(),
                ChecklistStavke = await _db.ChecklistStavke.Where(c => c.PlanId == id).Select(c => c.ToDto()).ToListAsync()
            };

            return Ok(fullPlan);
        }

        [HttpDelete("by-user/{userId}")]
        public async Task<IActionResult> DeleteAllForUser(Guid userId, [FromHeader(Name = "X-Internal-Key")] string? internalKey)
        {
            var expectedKey = _configuration["Internal:ApiKey"];
            if (string.IsNullOrEmpty(expectedKey) || internalKey != expectedKey)
            {
                return Unauthorized();
            }

            var plans = await _db.Plans.Where(p => p.KorisnikId == userId).ToListAsync();
            _db.Plans.RemoveRange(plans);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
