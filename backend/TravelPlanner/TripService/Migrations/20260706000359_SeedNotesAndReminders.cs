using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripService.Migrations
{
    /// <inheritdoc />
    public partial class SeedNotesAndReminders : Migration
    {
        // Isti planovi kao u SeedDemoTrips migraciji.
        private static readonly Guid PlanGrcka = Guid.Parse("10000000-0000-0000-0000-000000000001");
        private static readonly Guid PlanDubrovnik = Guid.Parse("10000000-0000-0000-0000-000000000002");
        private static readonly Guid PlanBeograd = Guid.Parse("10000000-0000-0000-0000-000000000003");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Beleske",
                columns: new[] { "Id", "PlanId", "Naslov", "Sadrzaj", "DatumKreiranja" },
                values: new object[,]
                {
                    {
                        Guid.Parse("60000000-0000-0000-0000-000000000001"), PlanGrcka, "Kontakt vlasnika apartmana",
                        "Ime: Kostas. Telefon: +30 691 234 5678. Javiti se dan pre dolaska.",
                        new DateTime(2026, 6, 2, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        Guid.Parse("60000000-0000-0000-0000-000000000002"), PlanGrcka, "Ideje za izlete",
                        "Meteora je oko 3h vožnje, razmisliti o jednodnevnom izletu ako ostane vremena.",
                        new DateTime(2026, 6, 3, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        Guid.Parse("60000000-0000-0000-0000-000000000003"), PlanBeograd, "Preporuke za restorane",
                        "Skadarlija za večeru, Ada Ciganlija za šetnju uz reku.",
                        new DateTime(2026, 6, 26, 0, 0, 0, DateTimeKind.Utc)
                    }
                });

            migrationBuilder.InsertData(
                table: "Podsetnici",
                columns: new[] { "Id", "PlanId", "Naziv", "Datum", "Opis", "Zavrseno" },
                values: new object[,]
                {
                    { Guid.Parse("70000000-0000-0000-0000-000000000001"), PlanGrcka, "Produžiti pasoš", new DateTime(2026, 7, 1), "Pasoš ističe za manje od 6 meseci od povratka.", true },
                    { Guid.Parse("70000000-0000-0000-0000-000000000002"), PlanGrcka, "Platiti drugu ratu apartmana", new DateTime(2026, 7, 20), (string)null, false },
                    { Guid.Parse("70000000-0000-0000-0000-000000000003"), PlanDubrovnik, "Rezervisati sto za večeru", new DateTime(2026, 9, 5), "Stara gradska luka, terasa sa pogledom.", false },
                    { Guid.Parse("70000000-0000-0000-0000-000000000004"), PlanBeograd, "Kupiti kartu za brod", new DateTime(2026, 8, 15), (string)null, false }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Podsetnici", keyColumn: "Id", keyValue: Guid.Parse("70000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(table: "Podsetnici", keyColumn: "Id", keyValue: Guid.Parse("70000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(table: "Podsetnici", keyColumn: "Id", keyValue: Guid.Parse("70000000-0000-0000-0000-000000000003"));
            migrationBuilder.DeleteData(table: "Podsetnici", keyColumn: "Id", keyValue: Guid.Parse("70000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(table: "Beleske", keyColumn: "Id", keyValue: Guid.Parse("60000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(table: "Beleske", keyColumn: "Id", keyValue: Guid.Parse("60000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(table: "Beleske", keyColumn: "Id", keyValue: Guid.Parse("60000000-0000-0000-0000-000000000003"));
        }
    }
}
