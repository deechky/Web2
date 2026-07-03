using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TripService.Data;
using TripService.Dtos;
using TripService.Services;

namespace TripService.Controllers
{
    [ApiController]
    [Route("api/trips/{tripId}/budget")]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly TripDbContext _db;
        private readonly PlanAccess _planAccess;

        public BudgetController(TripDbContext db, PlanAccess planAccess)
        {
            _db = db;
            _planAccess = planAccess;
        }

        [HttpGet]
        public async Task<ActionResult<BudgetDto>> Get(Guid tripId)
        {
            var plan = await _planAccess.FindAccessiblePlanAsync(tripId, User);
            if (plan == null)
            {
                return NotFound();
            }

            var potrosenoTroskovi = await _db.Troskovi
                .Where(t => t.PlanId == tripId)
                .SumAsync(t => (decimal?)t.Iznos) ?? 0;

            var potrosenoAktivnosti = await _db.Aktivnosti
                .Where(a => a.PlanId == tripId)
                .SumAsync(a => (decimal?)a.ProcenjeniTrosak) ?? 0;

            var potroseno = potrosenoTroskovi + potrosenoAktivnosti;

            var budget = new BudgetDto
            {
                Planirano = plan.PlaniraniBudzet,
                Potroseno = potroseno,
                Preostalo = plan.PlaniraniBudzet - potroseno
            };

            return Ok(budget);
        }
    }
}
