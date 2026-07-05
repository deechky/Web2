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
    [Route("api/trips/{tripId}/notes")]
    [Authorize]
    public class NotesController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public NotesController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<BeleskaDto>>> GetAll(Guid tripId)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var beleske = await _db.Beleske
                .Where(b => b.PlanId == tripId)
                .OrderByDescending(b => b.DatumKreiranja)
                .Select(b => b.ToDto())
                .ToListAsync();

            return Ok(beleske);
        }

        [HttpPost]
        public async Task<ActionResult<BeleskaDto>> Create(Guid tripId, BeleskaCreateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naslov) || string.IsNullOrWhiteSpace(dto.Sadrzaj))
            {
                return BadRequest(new { poruka = "Naslov i sadržaj su obavezni." });
            }

            var beleska = dto.ToEntity(tripId);
            _db.Beleske.Add(beleska);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { tripId, id = beleska.Id }, beleska.ToDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BeleskaDto>> GetById(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var beleska = await _db.Beleske.FirstOrDefaultAsync(b => b.Id == id && b.PlanId == tripId);
            if (beleska == null)
            {
                return NotFound();
            }

            return Ok(beleska.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<BeleskaDto>> Update(Guid tripId, Guid id, BeleskaUpdateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var beleska = await _db.Beleske.FirstOrDefaultAsync(b => b.Id == id && b.PlanId == tripId);
            if (beleska == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naslov) || string.IsNullOrWhiteSpace(dto.Sadrzaj))
            {
                return BadRequest(new { poruka = "Naslov i sadržaj su obavezni." });
            }

            beleska.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(beleska.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var beleska = await _db.Beleske.FirstOrDefaultAsync(b => b.Id == id && b.PlanId == tripId);
            if (beleska == null)
            {
                return NotFound();
            }

            _db.Beleske.Remove(beleska);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
