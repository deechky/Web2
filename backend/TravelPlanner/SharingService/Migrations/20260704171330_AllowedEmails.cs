using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SharingService.Migrations
{
    /// <inheritdoc />
    public partial class AllowedEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DozvoljeniEmails",
                table: "Shares",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DozvoljeniEmails",
                table: "Shares");
        }
    }
}
