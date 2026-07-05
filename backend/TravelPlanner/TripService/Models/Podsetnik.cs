using System;

namespace TripService.Models
{
    public class Podsetnik
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
        public bool Zavrseno { get; set; }
    }
}
