using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripService.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoTrips : Migration
    {
        // Isti korisnici kao u AuthService/SeedDemoUsers migraciji.
        private static readonly Guid MarkoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid AnaId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        private static readonly Guid PlanGrcka = Guid.Parse("10000000-0000-0000-0000-000000000001");
        private static readonly Guid PlanDubrovnik = Guid.Parse("10000000-0000-0000-0000-000000000002");
        private static readonly Guid PlanBeograd = Guid.Parse("10000000-0000-0000-0000-000000000003");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Plans",
                columns: new[] { "Id", "KorisnikId", "Naziv", "Opis", "PocetniDatum", "KrajnjiDatum", "PlaniraniBudzet", "Napomene", "DatumKreiranja" },
                values: new object[,]
                {
                    {
                        PlanGrcka, MarkoId, "Letovanje u Grčkoj", "Porodično letovanje na moru",
                        new DateTime(2026, 8, 1), new DateTime(2026, 8, 10), 800m,
                        "Poneti kremu za sunčanje", new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        PlanDubrovnik, MarkoId, "Vikend u Dubrovniku", "Kratak odmor van sezone",
                        new DateTime(2026, 9, 12), new DateTime(2026, 9, 14), 200m,
                        (string)null, new DateTime(2026, 6, 20, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        PlanBeograd, AnaId, "Vikend u Beogradu", "Obilazak prestonice",
                        new DateTime(2026, 8, 20), new DateTime(2026, 8, 22), 150m,
                        (string)null, new DateTime(2026, 6, 25, 0, 0, 0, DateTimeKind.Utc)
                    }
                });

            migrationBuilder.InsertData(
                table: "Destinacije",
                columns: new[] { "Id", "PlanId", "Naziv", "Lokacija", "DatumDolaska", "DatumOdlaska", "Opis" },
                values: new object[,]
                {
                    { Guid.Parse("20000000-0000-0000-0000-000000000001"), PlanGrcka, "Halkidiki", "Grčka", new DateTime(2026, 8, 1), new DateTime(2026, 8, 10), "Apartman na plaži" },
                    { Guid.Parse("20000000-0000-0000-0000-000000000002"), PlanGrcka, "Solun", "Grčka", new DateTime(2026, 8, 5), new DateTime(2026, 8, 6), "Jednodnevni izlet" },
                    { Guid.Parse("20000000-0000-0000-0000-000000000003"), PlanDubrovnik, "Dubrovnik", "Hrvatska", new DateTime(2026, 9, 12), new DateTime(2026, 9, 14), (string)null },
                    { Guid.Parse("20000000-0000-0000-0000-000000000004"), PlanBeograd, "Beograd", "Srbija", new DateTime(2026, 8, 20), new DateTime(2026, 8, 22), (string)null }
                });

            migrationBuilder.InsertData(
                table: "Aktivnosti",
                columns: new[] { "Id", "PlanId", "Naziv", "Datum", "Vreme", "Lokacija", "Opis", "ProcenjeniTrosak", "Status" },
                values: new object[,]
                {
                    { Guid.Parse("30000000-0000-0000-0000-000000000001"), PlanGrcka, "Obilazak plaže", new DateTime(2026, 8, 2), new TimeSpan(10, 0, 0), "Kriopigi", (string)null, 20m, "Planirano" },
                    { Guid.Parse("30000000-0000-0000-0000-000000000002"), PlanGrcka, "Izlet u Solun", new DateTime(2026, 8, 5), new TimeSpan(9, 0, 0), "Solun", "Razgledanje centra grada", 40m, "Rezervisano" },
                    { Guid.Parse("30000000-0000-0000-0000-000000000003"), PlanGrcka, "Večera u restoranu", new DateTime(2026, 8, 7), new TimeSpan(19, 30, 0), "Halkidiki", (string)null, 35m, "Planirano" },
                    { Guid.Parse("30000000-0000-0000-0000-000000000004"), PlanDubrovnik, "Šetnja gradskim zidinama", new DateTime(2026, 9, 13), new TimeSpan(10, 0, 0), "Dubrovnik", (string)null, 15m, "Planirano" },
                    { Guid.Parse("30000000-0000-0000-0000-000000000005"), PlanBeograd, "Poseta Kalemegdanu", new DateTime(2026, 8, 21), new TimeSpan(11, 0, 0), "Beograd", (string)null, 0m, "Planirano" },
                    { Guid.Parse("30000000-0000-0000-0000-000000000006"), PlanBeograd, "Vožnja brodom", new DateTime(2026, 8, 21), new TimeSpan(17, 0, 0), "Beograd", "Panoramska tura Dunavom i Savom", 25m, "Rezervisano" }
                });

            migrationBuilder.InsertData(
                table: "Troskovi",
                columns: new[] { "Id", "PlanId", "Naziv", "Kategorija", "Iznos", "Datum", "Opis" },
                values: new object[,]
                {
                    { Guid.Parse("40000000-0000-0000-0000-000000000001"), PlanGrcka, "Avionske karte", "Prevoz", 250m, new DateTime(2026, 7, 10), "Povratna karta za dvoje" },
                    { Guid.Parse("40000000-0000-0000-0000-000000000002"), PlanGrcka, "Apartman", "Smestaj", 400m, new DateTime(2026, 7, 15), "10 noćenja" },
                    { Guid.Parse("40000000-0000-0000-0000-000000000003"), PlanGrcka, "Putno osiguranje", "Ostalo", 30m, new DateTime(2026, 7, 12), (string)null },
                    { Guid.Parse("40000000-0000-0000-0000-000000000004"), PlanDubrovnik, "Hotel", "Smestaj", 120m, new DateTime(2026, 9, 1), (string)null },
                    { Guid.Parse("40000000-0000-0000-0000-000000000005"), PlanBeograd, "Autobuska karta", "Prevoz", 20m, new DateTime(2026, 8, 10), (string)null },
                    { Guid.Parse("40000000-0000-0000-0000-000000000006"), PlanBeograd, "Hostel", "Smestaj", 60m, new DateTime(2026, 8, 15), "2 noćenja" }
                });

            migrationBuilder.InsertData(
                table: "ChecklistStavke",
                columns: new[] { "Id", "PlanId", "Naziv", "Zavrseno" },
                values: new object[,]
                {
                    { Guid.Parse("50000000-0000-0000-0000-000000000001"), PlanGrcka, "Pasoš", true },
                    { Guid.Parse("50000000-0000-0000-0000-000000000002"), PlanGrcka, "Putno osiguranje", true },
                    { Guid.Parse("50000000-0000-0000-0000-000000000003"), PlanGrcka, "Kupaći kostim", false },
                    { Guid.Parse("50000000-0000-0000-0000-000000000004"), PlanGrcka, "Punjač za telefon", false },
                    { Guid.Parse("50000000-0000-0000-0000-000000000005"), PlanDubrovnik, "Rezervacija hotela", true },
                    { Guid.Parse("50000000-0000-0000-0000-000000000006"), PlanBeograd, "Karta za autobus", true },
                    { Guid.Parse("50000000-0000-0000-0000-000000000007"), PlanBeograd, "Fotoaparat", false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "Id", keyValue: PlanGrcka);
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "Id", keyValue: PlanDubrovnik);
            migrationBuilder.DeleteData(table: "Plans", keyColumn: "Id", keyValue: PlanBeograd);
        }
    }
}
