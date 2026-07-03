using System;

namespace TripService.Dtos
{
    public class TrosakCreateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public string Kategorija { get; set; } = "Ostalo";
        public decimal Iznos { get; set; }
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
    }

    public class TrosakUpdateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public string Kategorija { get; set; } = "Ostalo";
        public decimal Iznos { get; set; }
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
    }

    public class TrosakDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string Kategorija { get; set; } = string.Empty;
        public decimal Iznos { get; set; }
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
    }
}
