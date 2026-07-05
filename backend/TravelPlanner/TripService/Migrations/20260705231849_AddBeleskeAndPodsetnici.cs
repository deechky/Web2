using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TripService.Migrations
{
    /// <inheritdoc />
    public partial class AddBeleskeAndPodsetnici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Beleske",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Naslov = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sadrzaj = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    DatumKreiranja = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beleske", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Beleske_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Podsetnici",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Naziv = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Datum = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Opis = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Zavrseno = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Podsetnici", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Podsetnici_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Beleske_PlanId",
                table: "Beleske",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Podsetnici_PlanId",
                table: "Podsetnici",
                column: "PlanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Beleske");

            migrationBuilder.DropTable(
                name: "Podsetnici");
        }
    }
}
