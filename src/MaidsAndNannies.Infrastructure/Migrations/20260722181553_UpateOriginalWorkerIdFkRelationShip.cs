using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpateOriginalWorkerIdFkRelationShip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Users_OriginalWorkerId1",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_OriginalWorkerId1",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OriginalWorkerId1",
                table: "Bookings");

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_OriginalWorkerId",
                table: "Bookings",
                column: "OriginalWorkerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_WorkerProfiles_OriginalWorkerId",
                table: "Bookings",
                column: "OriginalWorkerId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_WorkerProfiles_OriginalWorkerId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_OriginalWorkerId",
                table: "Bookings");

            migrationBuilder.AddColumn<string>(
                name: "OriginalWorkerId1",
                table: "Bookings",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

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
    }
}
