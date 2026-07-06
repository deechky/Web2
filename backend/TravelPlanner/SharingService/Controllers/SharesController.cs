using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        private readonly ShareStore _shareStore;
        private readonly TripClient _tripClient;
        private readonly IConfiguration _configuration;

        public SharesController(ShareStore shareStore, TripClient tripClient, IConfiguration configuration)
        {
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

            var dozvoljeniEmails = ShareEmails.Normalize(dto.Emails);
            if (tip == ShareTip.Edit && dozvoljeniEmails == null)
            {
                return BadRequest(new { poruka = "Za EDIT deljenje unesite bar jedan email naloga sa pravom izmene." });
            }

            var defaultExpiryDays = _configuration.GetValue<int>("Sharing:DefaultExpiryDays");
            var share = await _shareStore.CreateAsync(tripId, User.GetKorisnikId(), tip, defaultExpiryDays, dozvoljeniEmails);

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

            var shares = await _shareStore.GetForPlanAsync(tripId);
            return Ok(shares.Select(s => s.ToDto()));
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

            var revoked = await _shareStore.RevokeAsync(tripId, id);
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
        public async Task<IActionResult> Validate(string code, [FromQuery] string? email)
        {
            var share = await _shareStore.ResolveAsync(code);
            if (share == null || share.Opozvan || (share.IstekDatum.HasValue && share.IstekDatum.Value < DateTime.UtcNow))
            {
                return Ok(new { valid = false });
            }

            if (!share.IsEmailAllowed(email))
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
