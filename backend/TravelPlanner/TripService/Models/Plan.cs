using System;
using System.Collections.Generic;

namespace TripService.Models
{
    public class Plan
    {
        public Guid Id { get; set; }
        public Guid KorisnikId { get; set; }
        public string Naziv { get; set; } = string.Empty;
        public string? Opis { get; set; }
        public DateTime PocetniDatum { get; set; }
        public DateTime KrajnjiDatum { get; set; }
        public decimal PlaniraniBudzet { get; set; }
        public string? Napomene { get; set; }
        public DateTime DatumKreiranja { get; set; } = DateTime.UtcNow;

        public List<Destinacija> Destinacije { get; set; } = new();
        public List<Aktivnost> Aktivnosti { get; set; } = new();
        public List<Trosak> Troskovi { get; set; } = new();
        public List<ChecklistStavka> ChecklistStavke { get; set; } = new();
        public List<Beleska> Beleske { get; set; } = new();
        public List<Podsetnik> Podsetnici { get; set; } = new();
    }
}
