using System;

namespace SharingService.Dtos
{
    public class ShareCreateDto
    {
        public string Tip { get; set; } = "View";
    }

    public class ShareDto
    {
        public Guid Id { get; set; }
        public string Kod { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public DateTime? IstekDatum { get; set; }
        public bool Opozvan { get; set; }
        public DateTime DatumKreiranja { get; set; }
    }
}
