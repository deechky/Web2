using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SharingService.Services;

namespace SharingService.Controllers
{
    [ApiController]
    [Route("api/internal/shares")]
    public class InternalController : ControllerBase
    {
        private readonly ShareStore _shareStore;
        private readonly IConfiguration _configuration;

        public InternalController(ShareStore shareStore, IConfiguration configuration)
        {
            _shareStore = shareStore;
            _configuration = configuration;
        }

        [HttpDelete("by-plan/{planId}")]
        public async Task<IActionResult> DeleteForPlan(Guid planId, [FromHeader(Name = "X-Internal-Key")] string? internalKey)
        {
            var expectedKey = _configuration["Internal:ApiKey"];
            if (string.IsNullOrEmpty(expectedKey) || internalKey != expectedKey)
            {
                return Unauthorized();
            }

            await _shareStore.DeleteForPlanAsync(planId);

            return NoContent();
        }
    }
}
