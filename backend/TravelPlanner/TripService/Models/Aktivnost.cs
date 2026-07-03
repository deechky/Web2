using System;

namespace TripService.Models
{
    public enum AktivnostStatus
    {
        Planirano,
        Rezervisano,
        Zavrseno,
        Otkazano
    }

    public class Aktivnost
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public TimeSpan? Vreme { get; set; }
        public string? Lokacija { get; set; }
        public string? Opis { get; set; }
        public decimal ProcenjeniTrosak { get; set; }
        public AktivnostStatus Status { get; set; } = AktivnostStatus.Planirano;
    }
}
