using System;

namespace TripService.Dtos
{
    public class DestinacijaCreateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public string Lokacija { get; set; } = string.Empty;
        public DateTime DatumDolaska { get; set; }
        public DateTime DatumOdlaska { get; set; }
        public string? Opis { get; set; }
    }

    public class DestinacijaUpdateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public string Lokacija { get; set; } = string.Empty;
        public DateTime DatumDolaska { get; set; }
        public DateTime DatumOdlaska { get; set; }
        public string? Opis { get; set; }
    }

    public class DestinacijaDto
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
