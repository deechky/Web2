using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TripService.Data;
using TripService.Models;
using TripService.Security;

namespace TripService.Services
{
    public class PlanAccess
    {
        private readonly TripDbContext _db;

        public PlanAccess(TripDbContext db)
        {
            _db = db;
        }

        public async Task<Plan?> FindAccessiblePlanAsync(Guid planId, ClaimsPrincipal user)
        {
            var plan = await _db.Plans.FirstOrDefaultAsync(p => p.Id == planId);
            if (plan == null)
            {
                return null;
            }

            if (plan.KorisnikId != user.GetKorisnikId() && !user.IsAdmin())
            {
                return null;
            }

            return plan;
        }
    }
}
