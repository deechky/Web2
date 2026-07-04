using System;
using SharingService.Models;

namespace SharingService.Services
{
    public class ShareCacheEntry
    {
        public Guid PlanId { get; set; }
        public string Tip { get; set; } = string.Empty;
        public string? DozvoljeniEmails { get; set; }
        public DateTime? IstekDatum { get; set; }
        public bool Opozvan { get; set; }

        public bool IsEmailAllowed(string? email)
        {
            if (!string.Equals(Tip, ShareTip.Edit.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return ShareEmails.Contains(DozvoljeniEmails, email);
        }
    }
}
