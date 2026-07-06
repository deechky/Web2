using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuthService.Dtos;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthLogic _authLogic;

        public AuthController(AuthLogic authLogic)
        {
            _authLogic = authLogic;
        }

        private static readonly Regex EmailRegex = new(@"^\S+@\S+\.\S+$", RegexOptions.Compiled);

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Ime) ||
                string.IsNullOrWhiteSpace(dto.Email) ||
                string.IsNullOrWhiteSpace(dto.Lozinka))
            {
                return BadRequest(new { poruka = "Ime, email i lozinka su obavezni." });
            }
            if (!EmailRegex.IsMatch(dto.Email))
            {
                return BadRequest(new { poruka = "Email nije u ispravnom formatu." });
            }
            if (dto.Lozinka.Length < 6)
            {
                return BadRequest(new { poruka = "Lozinka mora imati bar 6 karaktera." });
            }

            try
            {
                var response = await _authLogic.RegisterAsync(dto);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { poruka = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Lozinka))
            {
                return BadRequest(new { poruka = "Email i lozinka su obavezni." });
            }

            try
            {
                var response = await _authLogic.LoginAsync(dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { poruka = ex.Message });
            }
        }
    }
}
