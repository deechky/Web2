using System;

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
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
        public DateTime? IstekDatum { get; set; }
        public bool Opozvan { get; set; }
    }
}
