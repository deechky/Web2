using System;

namespace TripService.Models
{
    public class Destinacija
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Lokacija { get; set; } = string.Empty;
        public DateTime DatumDolaska { get; set; }
        public DateTime DatumOdlaska { get; set; }
        public string? Opis { get; set; }
    }
}
