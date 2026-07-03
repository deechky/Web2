using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly AuthDbContext _db;

        public UsersController(AuthDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetAll()
        {
            var users = await _db.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Ime = u.Ime,
                    Email = u.Email,
                    Uloga = u.Uloga.ToString(),
                    DatumKreiranja = u.DatumKreiranja
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { poruka = "Korisnik nije pronađen." });
            }

            // TODO: kad TripService bude gotov, ovde treba obrisati i sve planove ovog korisnika
            // (cross-service brisanje) — za sada se briše samo nalog u AuthService bazi.
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
