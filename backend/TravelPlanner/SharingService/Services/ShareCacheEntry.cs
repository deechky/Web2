using System;

namespace SharingService.Services
{
    public class ShareCacheEntry
    {
        public Guid PlanId { get; set; }
        public string Tip { get; set; } = string.Empty;
        public DateTime? IstekDatum { get; set; }
        public bool Opozvan { get; set; }
    }
}
