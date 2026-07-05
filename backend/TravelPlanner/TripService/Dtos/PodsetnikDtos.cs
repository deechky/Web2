using System;

namespace TripService.Dtos
{
    public class PodsetnikCreateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
    }

    public class PodsetnikUpdateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
        public bool Zavrseno { get; set; }
    }

    public class PodsetnikDto
    {
        public Guid Id { get; set; }
        public Guid PlanId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public DateTime Datum { get; set; }
        public string? Opis { get; set; }
        public bool Zavrseno { get; set; }
    }
}
