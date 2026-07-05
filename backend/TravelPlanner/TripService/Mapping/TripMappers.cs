using System;
using TripService.Dtos;
using TripService.Models;

namespace TripService.Mapping
{
    public static class TripMappers
    {
        public static PlanDto ToDto(this Plan plan) => new PlanDto
        {
            Id = plan.Id,
            KorisnikId = plan.KorisnikId,
            Naziv = plan.Naziv,
            Opis = plan.Opis,
            PocetniDatum = plan.PocetniDatum,
            KrajnjiDatum = plan.KrajnjiDatum,
            PlaniraniBudzet = plan.PlaniraniBudzet,
            Napomene = plan.Napomene,
            DatumKreiranja = plan.DatumKreiranja
        };

        public static void ApplyUpdate(this Plan plan, PlanUpdateDto dto)
        {
            plan.Naziv = dto.Naziv;
            plan.Opis = dto.Opis;
            plan.PocetniDatum = dto.PocetniDatum;
            plan.KrajnjiDatum = dto.KrajnjiDatum;
            plan.PlaniraniBudzet = dto.PlaniraniBudzet;
            plan.Napomene = dto.Napomene;
        }

        public static Plan ToEntity(this PlanCreateDto dto, Guid korisnikId) => new Plan
        {
            Id = Guid.NewGuid(),
            KorisnikId = korisnikId,
            Naziv = dto.Naziv,
            Opis = dto.Opis,
            PocetniDatum = dto.PocetniDatum,
            KrajnjiDatum = dto.KrajnjiDatum,
            PlaniraniBudzet = dto.PlaniraniBudzet,
            Napomene = dto.Napomene,
            DatumKreiranja = DateTime.UtcNow
        };

        public static DestinacijaDto ToDto(this Destinacija d) => new DestinacijaDto
        {
            Id = d.Id,
            PlanId = d.PlanId,
            Naziv = d.Naziv,
            Lokacija = d.Lokacija,
            DatumDolaska = d.DatumDolaska,
            DatumOdlaska = d.DatumOdlaska,
            Opis = d.Opis
        };

        public static void ApplyUpdate(this Destinacija d, DestinacijaUpdateDto dto)
        {
            d.Naziv = dto.Naziv;
            d.Lokacija = dto.Lokacija;
            d.DatumDolaska = dto.DatumDolaska;
            d.DatumOdlaska = dto.DatumOdlaska;
            d.Opis = dto.Opis;
        }

        public static Destinacija ToEntity(this DestinacijaCreateDto dto, Guid planId) => new Destinacija
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Naziv = dto.Naziv,
            Lokacija = dto.Lokacija,
            DatumDolaska = dto.DatumDolaska,
            DatumOdlaska = dto.DatumOdlaska,
            Opis = dto.Opis
        };

        public static AktivnostDto ToDto(this Aktivnost a) => new AktivnostDto
        {
            Id = a.Id,
            PlanId = a.PlanId,
            Naziv = a.Naziv,
            Datum = a.Datum,
            Vreme = a.Vreme,
            Lokacija = a.Lokacija,
            Opis = a.Opis,
            ProcenjeniTrosak = a.ProcenjeniTrosak,
            Status = a.Status.ToString()
        };

        public static void ApplyUpdate(this Aktivnost a, AktivnostUpdateDto dto)
        {
            a.Naziv = dto.Naziv;
            a.Datum = dto.Datum;
            a.Vreme = dto.Vreme;
            a.Lokacija = dto.Lokacija;
            a.Opis = dto.Opis;
            a.ProcenjeniTrosak = dto.ProcenjeniTrosak;
            a.Status = Enum.Parse<AktivnostStatus>(dto.Status);
        }

        public static Aktivnost ToEntity(this AktivnostCreateDto dto, Guid planId) => new Aktivnost
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Naziv = dto.Naziv,
            Datum = dto.Datum,
            Vreme = dto.Vreme,
            Lokacija = dto.Lokacija,
            Opis = dto.Opis,
            ProcenjeniTrosak = dto.ProcenjeniTrosak,
            Status = Enum.Parse<AktivnostStatus>(dto.Status)
        };

        public static TrosakDto ToDto(this Trosak t) => new TrosakDto
        {
            Id = t.Id,
            PlanId = t.PlanId,
            Naziv = t.Naziv,
            Kategorija = t.Kategorija.ToString(),
            Iznos = t.Iznos,
            Datum = t.Datum,
            Opis = t.Opis
        };

        public static void ApplyUpdate(this Trosak t, TrosakUpdateDto dto)
        {
            t.Naziv = dto.Naziv;
            t.Kategorija = Enum.Parse<TrosakKategorija>(dto.Kategorija);
            t.Iznos = dto.Iznos;
            t.Datum = dto.Datum;
            t.Opis = dto.Opis;
        }

        public static Trosak ToEntity(this TrosakCreateDto dto, Guid planId) => new Trosak
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Naziv = dto.Naziv,
            Kategorija = Enum.Parse<TrosakKategorija>(dto.Kategorija),
            Iznos = dto.Iznos,
            Datum = dto.Datum,
            Opis = dto.Opis
        };

        public static ChecklistStavkaDto ToDto(this ChecklistStavka c) => new ChecklistStavkaDto
        {
            Id = c.Id,
            PlanId = c.PlanId,
            Naziv = c.Naziv,
            Zavrseno = c.Zavrseno
        };

        public static void ApplyUpdate(this ChecklistStavka c, ChecklistStavkaUpdateDto dto)
        {
            c.Naziv = dto.Naziv;
            c.Zavrseno = dto.Zavrseno;
        }

        public static ChecklistStavka ToEntity(this ChecklistStavkaCreateDto dto, Guid planId) => new ChecklistStavka
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Naziv = dto.Naziv,
            Zavrseno = false
        };

        public static BeleskaDto ToDto(this Beleska b) => new BeleskaDto
        {
            Id = b.Id,
            PlanId = b.PlanId,
            Naslov = b.Naslov,
            Sadrzaj = b.Sadrzaj,
            DatumKreiranja = b.DatumKreiranja
        };

        public static void ApplyUpdate(this Beleska b, BeleskaUpdateDto dto)
        {
            b.Naslov = dto.Naslov;
            b.Sadrzaj = dto.Sadrzaj;
        }

        public static Beleska ToEntity(this BeleskaCreateDto dto, Guid planId) => new Beleska
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Naslov = dto.Naslov,
            Sadrzaj = dto.Sadrzaj,
            DatumKreiranja = DateTime.UtcNow
        };

        public static PodsetnikDto ToDto(this Podsetnik r) => new PodsetnikDto
        {
            Id = r.Id,
            PlanId = r.PlanId,
            Naziv = r.Naziv,
            Datum = r.Datum,
            Opis = r.Opis,
            Zavrseno = r.Zavrseno
        };

        public static void ApplyUpdate(this Podsetnik r, PodsetnikUpdateDto dto)
        {
            r.Naziv = dto.Naziv;
            r.Datum = dto.Datum;
            r.Opis = dto.Opis;
            r.Zavrseno = dto.Zavrseno;
        }

        public static Podsetnik ToEntity(this PodsetnikCreateDto dto, Guid planId) => new Podsetnik
        {
            Id = Guid.NewGuid(),
            PlanId = planId,
            Naziv = dto.Naziv,
            Datum = dto.Datum,
            Opis = dto.Opis,
            Zavrseno = false
        };
    }
}
