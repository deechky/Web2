using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TripService.Data;
using TripService.Dtos;
using TripService.Mapping;
using TripService.Services;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/internal/trips")]
    public class InternalController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly IConfiguration _configuration;
        private readonly ShareClient _shareClient;

        public InternalController(TripDbContext db, IConfiguration configuration, ShareClient shareClient)
        {
            _db = db;
            _configuration = configuration;
            _shareClient = shareClient;
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

            var potrosenoTroskovi = await _db.Troskovi.Where(t => t.PlanId == id).SumAsync(t => (decimal?)t.Iznos) ?? 0;
            var potrosenoAktivnosti = await _db.Aktivnosti.Where(a => a.PlanId == id).SumAsync(a => (decimal?)a.ProcenjeniTrosak) ?? 0;
            var potroseno = potrosenoTroskovi + potrosenoAktivnosti;

            var fullPlan = new FullPlanDto
            {
                Plan = plan.ToDto(),
                Destinacije = await _db.Destinacije.Where(d => d.PlanId == id).Select(d => d.ToDto()).ToListAsync(),
                Aktivnosti = await _db.Aktivnosti.Where(a => a.PlanId == id).Select(a => a.ToDto()).ToListAsync(),
                Troskovi = await _db.Troskovi.Where(t => t.PlanId == id).Select(t => t.ToDto()).ToListAsync(),
                ChecklistStavke = await _db.ChecklistStavke.Where(c => c.PlanId == id).Select(c => c.ToDto()).ToListAsync(),
                Beleske = await _db.Beleske.Where(b => b.PlanId == id).OrderByDescending(b => b.DatumKreiranja).Select(b => b.ToDto()).ToListAsync(),
                Podsetnici = await _db.Podsetnici.Where(r => r.PlanId == id).OrderBy(r => r.Datum).Select(r => r.ToDto()).ToListAsync(),
                Budzet = new BudgetDto
                {
                    Planirano = plan.PlaniraniBudzet,
                    Potroseno = potroseno,
                    Preostalo = plan.PlaniraniBudzet - potroseno
                }
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

            foreach (var plan in plans)
            {
                await _shareClient.DeleteSharesForPlanAsync(plan.Id);
            }

            return NoContent();
        }
    }
}
