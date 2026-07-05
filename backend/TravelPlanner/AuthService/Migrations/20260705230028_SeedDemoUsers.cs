using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoUsers : Migration
    {
        public static readonly Guid MarkoId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid AnaId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Ime", "Email", "LozinkaHash", "Uloga", "DatumKreiranja" },
                values: new object[,]
                {
                    {
                        MarkoId,
                        "Marko Marković",
                        "marko@primer.com",
                        BCrypt.Net.BCrypt.HashPassword("marko123"),
                        "Korisnik",
                        new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
                    },
                    {
                        AnaId,
                        "Ana Anić",
                        "ana@primer.com",
                        BCrypt.Net.BCrypt.HashPassword("ana123"),
                        "Korisnik",
                        new DateTime(2026, 2, 5, 0, 0, 0, DateTimeKind.Utc)
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValue: MarkoId);
            migrationBuilder.DeleteData(table: "Users", keyColumn: "Id", keyValue: AnaId);
        }
    }
}
