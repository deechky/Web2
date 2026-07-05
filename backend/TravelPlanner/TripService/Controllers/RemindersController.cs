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
    [Route("api/trips/{tripId}/reminders")]
    [Authorize]
    public class RemindersController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public RemindersController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<PodsetnikDto>>> GetAll(Guid tripId)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var podsetnici = await _db.Podsetnici
                .Where(r => r.PlanId == tripId)
                .OrderBy(r => r.Datum)
                .Select(r => r.ToDto())
                .ToListAsync();

            return Ok(podsetnici);
        }

        [HttpPost]
        public async Task<ActionResult<PodsetnikDto>> Create(Guid tripId, PodsetnikCreateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv podsetnika je obavezan." });
            }

            var podsetnik = dto.ToEntity(tripId);
            _db.Podsetnici.Add(podsetnik);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { tripId }, podsetnik.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PodsetnikDto>> Update(Guid tripId, Guid id, PodsetnikUpdateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var podsetnik = await _db.Podsetnici.FirstOrDefaultAsync(r => r.Id == id && r.PlanId == tripId);
            if (podsetnik == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv podsetnika je obavezan." });
            }

            podsetnik.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(podsetnik.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var podsetnik = await _db.Podsetnici.FirstOrDefaultAsync(r => r.Id == id && r.PlanId == tripId);
            if (podsetnik == null)
            {
                return NotFound();
            }

            _db.Podsetnici.Remove(podsetnik);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
