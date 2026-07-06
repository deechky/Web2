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
using TripService.Services;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/trips/{tripId}/checklist-items")]
    [Authorize]
    public class ChecklistItemsController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public ChecklistItemsController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<ChecklistStavkaDto>>> GetAll(Guid tripId)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var stavke = await _db.ChecklistStavke
                .Where(c => c.PlanId == tripId)
                .Select(c => c.ToDto())
                .ToListAsync();

            return Ok(stavke);
        }

        [HttpPost]
        public async Task<ActionResult<ChecklistStavkaDto>> Create(Guid tripId, ChecklistStavkaCreateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv stavke je obavezan." });
            }

            var stavka = dto.ToEntity(tripId);
            _db.ChecklistStavke.Add(stavka);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { tripId }, stavka.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ChecklistStavkaDto>> Update(Guid tripId, Guid id, ChecklistStavkaUpdateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var stavka = await _db.ChecklistStavke.FirstOrDefaultAsync(c => c.Id == id && c.PlanId == tripId);
            if (stavka == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv stavke je obavezan." });
            }

            stavka.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(stavka.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var stavka = await _db.ChecklistStavke.FirstOrDefaultAsync(c => c.Id == id && c.PlanId == tripId);
            if (stavka == null)
            {
                return NotFound();
            }

            _db.ChecklistStavke.Remove(stavka);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
