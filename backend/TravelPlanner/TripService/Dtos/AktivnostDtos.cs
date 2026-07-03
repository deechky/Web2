using System;

namespace TripService.Dtos
{
    public class AktivnostCreateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public TimeSpan? Vreme { get; set; }
        public string? Lokacija { get; set; }
        public string? Opis { get; set; }
        public decimal ProcenjeniTrosak { get; set; }
        public string Status { get; set; } = "Planirano";
    }

    public class AktivnostUpdateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public TimeSpan? Vreme { get; set; }
        public string? Lokacija { get; set; }
        public string? Opis { get; set; }
        public decimal ProcenjeniTrosak { get; set; }
        public string Status { get; set; } = "Planirano";
    }

    public class AktivnostDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public TimeSpan? Vreme { get; set; }
        public string? Lokacija { get; set; }
        public string? Opis { get; set; }
        public decimal ProcenjeniTrosak { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
