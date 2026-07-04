using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharingService.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KreatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IstekDatum = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Opozvan = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shares", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shares_Kod",
                table: "Shares",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shares_PlanId",
                table: "Shares",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shares");
        }
    }
}
