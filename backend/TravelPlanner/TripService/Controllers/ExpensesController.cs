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
    [Route("api/trips/{tripId}/expenses")]
    [Authorize]
    public class ExpensesController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public ExpensesController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<TrosakDto>>> GetAll(Guid tripId)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var troskovi = await _db.Troskovi
                .Where(t => t.PlanId == tripId)
                .Select(t => t.ToDto())
                .ToListAsync();

            return Ok(troskovi);
        }

        [HttpPost]
        public async Task<ActionResult<TrosakDto>> Create(Guid tripId, TrosakCreateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv troška je obavezan." });
            }
            if (dto.Iznos < 0)
            {
                return BadRequest(new { poruka = "Iznos ne može biti negativan." });
            }
            if (!Enum.TryParse<TrosakKategorija>(dto.Kategorija, out _))
            {
                return BadRequest(new { poruka = "Nepoznata kategorija troška." });
            }

            var trosak = dto.ToEntity(tripId);
            _db.Troskovi.Add(trosak);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { tripId, id = trosak.Id }, trosak.ToDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TrosakDto>> GetById(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var trosak = await _db.Troskovi.FirstOrDefaultAsync(t => t.Id == id && t.PlanId == tripId);
            if (trosak == null)
            {
                return NotFound();
            }

            return Ok(trosak.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<TrosakDto>> Update(Guid tripId, Guid id, TrosakUpdateDto dto)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var trosak = await _db.Troskovi.FirstOrDefaultAsync(t => t.Id == id && t.PlanId == tripId);
            if (trosak == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv))
            {
                return BadRequest(new { poruka = "Naziv troška je obavezan." });
            }
            if (dto.Iznos < 0)
            {
                return BadRequest(new { poruka = "Iznos ne može biti negativan." });
            }
            if (!Enum.TryParse<TrosakKategorija>(dto.Kategorija, out _))
            {
                return BadRequest(new { poruka = "Nepoznata kategorija troška." });
            }

            trosak.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(trosak.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var trosak = await _db.Troskovi.FirstOrDefaultAsync(t => t.Id == id && t.PlanId == tripId);
            if (trosak == null)
            {
                return NotFound();
            }

            _db.Troskovi.Remove(trosak);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
