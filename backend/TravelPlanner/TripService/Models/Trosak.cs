using System;

namespace TripService.Models
{
    public enum TrosakKategorija
    {
        Prevoz,
        Smestaj,
        Hrana,
        Ulaznice,
        Kupovina,
        Ostalo
    }

    public class Trosak
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public TrosakKategorija Kategorija { get; set; } = TrosakKategorija.Ostalo;
        public decimal Iznos { get; set; }
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
    }
}
