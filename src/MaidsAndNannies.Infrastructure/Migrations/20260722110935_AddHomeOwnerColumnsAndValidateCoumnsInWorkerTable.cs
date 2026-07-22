using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeOwnerColumnsAndValidateCoumnsInWorkerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppNumber",
                table: "WorkerProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "HomeownerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppNumber",
                table: "HomeownerProfiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "HomeownerProfiles");

            migrationBuilder.DropColumn(
                name: "WhatsAppNumber",
                table: "HomeownerProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "WhatsAppNumber",
                table: "WorkerProfiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
