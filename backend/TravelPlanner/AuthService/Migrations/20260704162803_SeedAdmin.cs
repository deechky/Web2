using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdmin : Migration
    {
        private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Ime", "Email", "LozinkaHash", "Uloga", "DatumKreiranja" },
                values: new object[]
                {
                    AdminId,
                    "Administrator",
                    "admin@travelplanner.com",
                    BCrypt.Net.BCrypt.HashPassword("admin123"),
                    "Admin",
                    new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: AdminId);
        }
    }
}
