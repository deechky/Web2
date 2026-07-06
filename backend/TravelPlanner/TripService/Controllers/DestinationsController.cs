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
    [Route("api/trips/{tripId}/destinations")]
    [Authorize]
    public class DestinationsController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public DestinationsController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<List<DestinacijaDto>>> GetAll(Guid tripId)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var destinacije = await _db.Destinacije
                .Where(d => d.PlanId == tripId)
                .Select(d => d.ToDto())
                .ToListAsync();

            return Ok(destinacije);
        }

        [HttpPost]
        public async Task<ActionResult<DestinacijaDto>> Create(Guid tripId, DestinacijaCreateDto dto)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(tripId, User);
            if (plan == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv) || string.IsNullOrWhiteSpace(dto.Lokacija))
            {
                return BadRequest(new { poruka = "Naziv i lokacija su obavezni." });
            }
            if (dto.DatumDolaska > dto.DatumOdlaska)
            {
                return BadRequest(new { poruka = "Datum dolaska ne može biti posle datuma odlaska." });
            }
            if (dto.DatumDolaska < plan.PocetniDatum || dto.DatumOdlaska > plan.KrajnjiDatum)
            {
                return BadRequest(new { poruka = "Datumi destinacije moraju biti u okviru trajanja plana." });
            }

            var destinacija = dto.ToEntity(tripId);
            _db.Destinacije.Add(destinacija);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { tripId, id = destinacija.Id }, destinacija.ToDto());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DestinacijaDto>> GetById(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var destinacija = await _db.Destinacije.FirstOrDefaultAsync(d => d.Id == id && d.PlanId == tripId);
            if (destinacija == null)
            {
                return NotFound();
            }

            return Ok(destinacija.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<DestinacijaDto>> Update(Guid tripId, Guid id, DestinacijaUpdateDto dto)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(tripId, User);
            if (plan == null)
            {
                return NotFound();
            }

            var destinacija = await _db.Destinacije.FirstOrDefaultAsync(d => d.Id == id && d.PlanId == tripId);
            if (destinacija == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(dto.Naziv) || string.IsNullOrWhiteSpace(dto.Lokacija))
            {
                return BadRequest(new { poruka = "Naziv i lokacija su obavezni." });
            }
            if (dto.DatumDolaska > dto.DatumOdlaska)
            {
                return BadRequest(new { poruka = "Datum dolaska ne može biti posle datuma odlaska." });
            }
            if (dto.DatumDolaska < plan.PocetniDatum || dto.DatumOdlaska > plan.KrajnjiDatum)
            {
                return BadRequest(new { poruka = "Datumi destinacije moraju biti u okviru trajanja plana." });
            }

            destinacija.ApplyUpdate(dto);
            await _db.SaveChangesAsync();

            return Ok(destinacija.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid tripId, Guid id)
        {
            if (await _planAccess.FindAccessiblePlanAsync(tripId, User) == null)
            {
                return NotFound();
            }

            var destinacija = await _db.Destinacije.FirstOrDefaultAsync(d => d.Id == id && d.PlanId == tripId);
            if (destinacija == null)
            {
                return NotFound();
            }

            _db.Destinacije.Remove(destinacija);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
