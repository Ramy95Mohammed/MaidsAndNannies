using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBokkingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNotes",
                table: "Bookings",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OriginalWorkerId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalWorkerId1",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ReplacementCount",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OriginalWorkerId1",
                table: "Bookings",
                column: "OriginalWorkerId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Users_OriginalWorkerId1",
                table: "Bookings",
                column: "OriginalWorkerId1",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_OriginalWorkerId1",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_OriginalWorkerId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "AdminNotes",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OriginalWorkerId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OriginalWorkerId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReplacementCount",
                table: "Bookings");
        }
    }
}
