using System;
using System.Threading.Tasks;
using AuthService.Data;
using AuthService.Dtos;
using AuthService.Models;
using AuthService.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{
    public class AuthLogic
    {
        private readonly AuthDbContext _db;
        private readonly JwtTokenService _jwtTokenService;

        public AuthLogic(AuthDbContext db, JwtTokenService jwtTokenService)
        {
            _db = db;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var emailZauzet = await _db.Users.AnyAsync(u => u.Email == dto.Email);
            if (emailZauzet)
            {
                throw new InvalidOperationException("Nalog sa ovim email-om već postoji.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Ime = dto.Ime,
                Email = dto.Email,
                LozinkaHash = PasswordHasher.Hash(dto.Lozinka),
                Uloga = UserRole.Korisnik,
                DatumKreiranja = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return BuildResponse(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !PasswordHasher.Verify(dto.Lozinka, user.LozinkaHash))
            {
                throw new UnauthorizedAccessException("Pogrešan email ili lozinka.");
            }

            return BuildResponse(user);
        }

        private AuthResponseDto BuildResponse(User user)
        {
            return new AuthResponseDto
            {
                Token = _jwtTokenService.GenerateToken(user),
                Ime = user.Ime,
                Email = user.Email,
                Uloga = user.Uloga.ToString()
            };
        }
    }
}
