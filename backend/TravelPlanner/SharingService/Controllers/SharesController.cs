using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharingService.Data;
using SharingService.Dtos;
using SharingService.Mapping;
using SharingService.Models;
using SharingService.Security;
using SharingService.Services;

namespace SharingService.Controllers
{
    [ApiController]
    public class SharesController : ControllerBase
    {
        private readonly SharingDbContext _db;
        private readonly ShareStore _shareStore;
        private readonly TripClient _tripClient;
        private readonly IConfiguration _configuration;

        public SharesController(SharingDbContext db, ShareStore shareStore, TripClient tripClient, IConfiguration configuration)
        {
            _db = db;
            _shareStore = shareStore;
            _tripClient = tripClient;
            _configuration = configuration;
        }

        [HttpPost("api/trips/{tripId}/shares")]
        [Authorize]
        public async Task<ActionResult<ShareDto>> Create(Guid tripId, ShareCreateDto dto)
        {
            var token = GetBearerToken();
            if (token == null || !await _tripClient.CanShareAsync(tripId, token))
            {
                return Forbid();
            }

            if (!Enum.TryParse<ShareTip>(dto.Tip, out var tip))
            {
                return BadRequest(new { poruka = "Nepoznat tip deljenja." });
            }

            var defaultExpiryDays = _configuration.GetValue<int>("Sharing:DefaultExpiryDays");
            var share = await _shareStore.CreateAsync(tripId, User.GetKorisnikId(), tip, defaultExpiryDays);

            return Created($"/api/trips/{tripId}/shares/{share.Id}", share.ToDto());
        }

        [HttpGet("api/trips/{tripId}/shares")]
        [Authorize]
        public async Task<ActionResult<List<ShareDto>>> GetAll(Guid tripId)
        {
            var token = GetBearerToken();
            if (token == null || !await _tripClient.CanShareAsync(tripId, token))
            {
                return Forbid();
            }

            var shares = await _db.Shares
                .Where(s => s.PlanId == tripId)
                .Select(s => s.ToDto())
                .ToListAsync();

            return Ok(shares);
        }

        [HttpDelete("api/trips/{tripId}/shares/{id}")]
        [Authorize]
        public async Task<IActionResult> Revoke(Guid tripId, Guid id)
        {
            var token = GetBearerToken();
            if (token == null || !await _tripClient.CanShareAsync(tripId, token))
            {
                return Forbid();
            }

            var revoked = await _shareStore.RevokeAsync(id);
            if (!revoked)
            {
                return NotFound();
            }

            return NoContent();
        }

        [HttpGet("api/shares/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> Resolve(string code)
        {
            var share = await _shareStore.ResolveAsync(code);
            if (share == null || share.Opozvan)
            {
                return NotFound();
            }

            if (share.IstekDatum.HasValue && share.IstekDatum.Value < DateTime.UtcNow)
            {
                return StatusCode(410);
            }

            if (share.Tip == ShareTip.View)
            {
                var plan = await _tripClient.GetFullPlanAsync(share.PlanId);
                if (plan == null)
                {
                    return NotFound();
                }

                return Ok(new { planId = share.PlanId, tip = share.Tip.ToString(), plan = plan.Value });
            }

            return Ok(new { planId = share.PlanId, tip = share.Tip.ToString() });
        }

        [HttpGet("api/shares/{code}/validate")]
        [AllowAnonymous]
        public async Task<IActionResult> Validate(string code)
        {
            var share = await _shareStore.ResolveAsync(code);
            if (share == null || share.Opozvan || (share.IstekDatum.HasValue && share.IstekDatum.Value < DateTime.UtcNow))
            {
                return Ok(new { valid = false });
            }

            return Ok(new { planId = share.PlanId, tip = share.Tip.ToString(), valid = true });
        }

        private string? GetBearerToken()
        {
            var header = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Bearer "))
            {
                return null;
            }

            return header.Substring("Bearer ".Length);
        }
    }
}
