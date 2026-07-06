using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripService.Data;
using TripService.Dtos;
using TripService.Mapping;
using TripService.Security;
using TripService.Services;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/trips")]
    [Authorize]
    public class TripsController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;
        private readonly ShareClient _shareClient;

        public TripsController(TripDbContext db, PlanAccess planAccess, ShareClient shareClient)
        {
            _db = db;
            _planAccess = planAccess;
            _shareClient = shareClient;
        }

        [HttpGet]
        public async Task<ActionResult<List<PlanDto>>> GetAll()
        {
            var query = _db.Plans.AsQueryable();

            if (!User.IsAdmin())
            {
                var korisnikId = User.GetKorisnikId();
                query = query.Where(p => p.KorisnikId == korisnikId);
            }

            var planovi = await query.Select(p => p.ToDto()).ToListAsync();
            return Ok(planovi);
        }

        [HttpPost]
        public async Task<ActionResult<PlanDto>> Create(PlanCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv plana je obavezan." });
            }
            if (dto.KrajnjiDatum < dto.PocetniDatum)
            {
                return BadRequest(new { poruka = "Krajnji datum ne može biti pre početnog." });
            }
            if (dto.PlaniraniBudzet < 0)
            {
                return BadRequest(new { poruka = "Budžet ne može biti negativan." });
            }

            var plan = dto.ToEntity(User.GetKorisnikId());
            _db.Plans.Add(plan);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan.ToDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlanDto>> GetById(Guid id)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(id, User);
            if (plan == null)
            {
                return NotFound();
            }

            return Ok(plan.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PlanDto>> Update(Guid id, PlanUpdateDto dto)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(id, User);
            if (plan == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv plana je obavezan." });
            }
            if (dto.KrajnjiDatum < dto.PocetniDatum)
            {
                return BadRequest(new { poruka = "Krajnji datum ne može biti pre početnog." });
            }
            if (dto.PlaniraniBudzet < 0)
            {
                return BadRequest(new { poruka = "Budžet ne može biti negativan." });
            }

            plan.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(plan.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(id, User);
            if (plan == null)
            {
                return NotFound();
            }

            _db.Plans.Remove(plan);
            await _db.SaveChangesAsync();

            await _shareClient.DeleteSharesForPlanAsync(id);

            return NoContent();
        }
    }
}
