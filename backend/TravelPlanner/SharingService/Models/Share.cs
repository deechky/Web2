using System;
using System.Collections.Generic;
using System.Linq;

namespace SharingService.Models
{
    public enum ShareTip
    {
        View,
        Edit
    }

    public class Share
    {
        public Guid Id { get; set; }
        public string Kod { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public Guid KreatorId { get; set; }
        public ShareTip Tip { get; set; } = ShareTip.View;

        // Samo za EDIT: mejlovi naloga sa pravom izmene, normalizovano i razdvojeno zarezom.
        // VIEW je javan (svako sa kodom vidi), pa ovde ostaje null.
        public string? DozvoljeniEmails { get; set; }

        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        public DateTime? IstekDatum { get; set; }
        public bool Opozvan { get; set; }

        public bool IsEmailAllowed(string? email)
        {
            if (Tip != ShareTip.Edit)
            {
                return true;
            }

            return ShareEmails.Contains(DozvoljeniEmails, email);
        }
    }

    public static class ShareEmails
    {
        public static string? Normalize(IEnumerable<string>? emails)
        {
            if (emails == null)
            {
                return null;
            }

            var normalized = emails
                .Select(e => e?.Trim().ToLowerInvariant())
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();

            return normalized.Count > 0 ? string.Join(",", normalized) : null;
        }

        public static bool Contains(string? csv, string? email)
        {
            if (string.IsNullOrWhiteSpace(csv) || string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var target = email.Trim().ToLowerInvariant();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Any(e => e == target);
        }

        public static List<string> ToList(string? csv) =>
            string.IsNullOrWhiteSpace(csv)
                ? new List<string>()
                : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}
