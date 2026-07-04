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
using TripService.Models;
using TripService.Services;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/trips/{tripId}/activities")]
    [Authorize]
    public class ActivitiesController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public ActivitiesController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<AktivnostDto>>> GetAll(Guid tripId, [FromQuery] DateTime? date)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var query = _db.Aktivnosti.Where(a => a.PlanId == tripId);
            if (date.HasValue)
            {
                query = query.Where(a => a.Datum.Date == date.Value.Date);
            }

            var aktivnosti = await query.Select(a => a.ToDto()).ToListAsync();
            return Ok(aktivnosti);
        }

        [HttpPost]
        public async Task<ActionResult<AktivnostDto>> Create(Guid tripId, AktivnostCreateDto dto)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(tripId, User);
            if (plan == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv aktivnosti je obavezan." });
            }
            if (dto.ProcenjeniTrosak < 0)
            {
                return BadRequest(new { poruka = "Procenjeni trošak ne može biti negativan." });
            }
            if (!Enum.TryParse<AktivnostStatus>(dto.Status, out _))
            {
                return BadRequest(new { poruka = "Nepoznat status aktivnosti." });
            }
            if (dto.Datum < plan.PocetniDatum || dto.Datum > plan.KrajnjiDatum)
            {
                return BadRequest(new { poruka = "Datum aktivnosti mora biti u okviru trajanja plana." });
            }

            var aktivnost = dto.ToEntity(tripId);
            _db.Aktivnosti.Add(aktivnost);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { tripId, id = aktivnost.Id }, aktivnost.ToDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AktivnostDto>> GetById(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var aktivnost = await _db.Aktivnosti.FirstOrDefaultAsync(a => a.Id == id && a.PlanId == tripId);
            if (aktivnost == null)
            {
                return NotFound();
            }

            return Ok(aktivnost.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AktivnostDto>> Update(Guid tripId, Guid id, AktivnostUpdateDto dto)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(tripId, User);
            if (plan == null)
            {
                return NotFound();
            }

            var aktivnost = await _db.Aktivnosti.FirstOrDefaultAsync(a => a.Id == id && a.PlanId == tripId);
            if (aktivnost == null)
            {
                return NotFound();
            }

            if (dto.ProcenjeniTrosak < 0)
            {
                return BadRequest(new { poruka = "Procenjeni trošak ne može biti negativan." });
            }
            if (!Enum.TryParse<AktivnostStatus>(dto.Status, out _))
            {
                return BadRequest(new { poruka = "Nepoznat status aktivnosti." });
            }
            if (dto.Datum < plan.PocetniDatum || dto.Datum > plan.KrajnjiDatum)
            {
                return BadRequest(new { poruka = "Datum aktivnosti mora biti u okviru trajanja plana." });
            }

            aktivnost.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(aktivnost.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var aktivnost = await _db.Aktivnosti.FirstOrDefaultAsync(a => a.Id == id && a.PlanId == tripId);
            if (aktivnost == null)
            {
                return NotFound();
            }

            _db.Aktivnosti.Remove(aktivnost);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
