using System;
using System.Security.Claims;

namespace TripService.Security
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid GetKorisnikId(this ClaimsPrincipal user)
        {
            var value = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(value!);
        }

        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole("Admin");
        }
    }
}
