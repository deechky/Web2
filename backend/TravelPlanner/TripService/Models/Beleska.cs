using System;

namespace TripService.Models
{
    public class Beleska
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naslov { get; set; } = string.Empty;
        public string Sadrzaj { get; set; } = string.Empty;
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;
    }
}
