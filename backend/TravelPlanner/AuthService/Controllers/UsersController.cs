using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dtos;
using AuthService.Services;
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
        private readonly TripClient _tripClient;

        public UsersController(AuthDbContext db, TripClient tripClient)
        {
            _db = db;
            _tripClient = tripClient;
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

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            await _tripClient.DeleteUserTripsAsync(id);

            return NoContent();
        }
    }
}
