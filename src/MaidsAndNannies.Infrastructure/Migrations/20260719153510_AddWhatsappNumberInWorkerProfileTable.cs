using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsappNumberInWorkerProfileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "WorkerProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "WorkerProfiles");
        }
    }
}
