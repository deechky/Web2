using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TripService.Data;
using TripService.Models;
using TripService.Security;

namespace TripService.Services
{
    public class PlanAccess
    {
        private readonly TripDbContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ShareClient _shareClient;

        public PlanAccess(TripDbContext db, IHttpContextAccessor httpContextAccessor, ShareClient shareClient)
        {
            _db = db;
            _httpContextAccessor = httpContextAccessor;
            _shareClient = shareClient;
        }

        public async Task<Plan?> FindAccessiblePlanAsync(Guid planId, ClaimsPrincipal user)
        {
            var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
            {
                return null;
            }

            if (plan.KorisnikId == user.GetKorisnikId() || user.IsAdmin())
            {
                return plan;
            }

            if (await HasValidEditShareAsync(planId))
            {
                return plan;
            }

            return null;
        }

        private async Task<bool> HasValidEditShareAsync(Guid planId)
        {
            var shareCode = _httpContextAccessor.HttpContext?.Request.Headers["X-Share-Code"].ToString();
            if (string.IsNullOrEmpty(shareCode))
            {
                return false;
            }

            var result = await _shareClient.ValidateAsync(shareCode);
            return result != null
                && result.Valid
                && result.PlanId == planId
                && string.Equals(result.Tip, "Edit", StringComparison.OrdinalIgnoreCase);
        }
    }
}
