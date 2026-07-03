using System;

namespace TripService.Dtos
{
    public class PlanCreateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public DateTime PocetniDatum { get; set; }
        public DateTime KrajnjiDatum { get; set; }
        public decimal PlaniraniBudzet { get; set; }
        public string? Napomene { get; set; }
    }

    public class PlanUpdateDto
    {
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public DateTime PocetniDatum { get; set; }
        public DateTime KrajnjiDatum { get; set; }
        public decimal PlaniraniBudzet { get; set; }
        public string? Napomene { get; set; }
    }

    public class PlanDto
    {
        public Guid Id { get; set; }
        public Guid KorisnikId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public DateTime PocetniDatum { get; set; }
        public DateTime KrajnjiDatum { get; set; }
        public decimal PlaniraniBudzet { get; set; }
        public string? Napomene { get; set; }
        public DateTime DatumKreiranja { get; set; }
    }
}
