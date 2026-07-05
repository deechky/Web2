using System;

namespace TripService.Dtos
{
    public class BeleskaCreateDto
    {
        public string Naslov { get; set; } = string.Empty;
        public string Sadrzaj { get; set; } = string.Empty;
    }

    public class BeleskaUpdateDto
    {
        public string Naslov { get; set; } = string.Empty;
        public string Sadrzaj { get; set; } = string.Empty;
    }

    public class BeleskaDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Sadrzaj { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; }
    }
}
