using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobPostIdColumnInBokkingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobPostId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 43, 32, 300, DateTimeKind.Utc).AddTicks(8220));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 43, 32, 300, DateTimeKind.Utc).AddTicks(8230));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 43, 32, 300, DateTimeKind.Utc).AddTicks(8242));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_JobPostId",
                table: "Bookings",
                column: "JobPostId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_JobPosts_JobPostId",
                table: "Bookings",
                column: "JobPostId",
                principalTable: "JobPosts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_JobPosts_JobPostId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_JobPostId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "JobPostId",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 12, 55, 766, DateTimeKind.Utc).AddTicks(817));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 12, 55, 766, DateTimeKind.Utc).AddTicks(827));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 12, 55, 766, DateTimeKind.Utc).AddTicks(835));
        }
    }
}
