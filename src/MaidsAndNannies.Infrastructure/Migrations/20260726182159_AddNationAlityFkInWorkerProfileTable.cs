using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNationAlityFkInWorkerProfileTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 18, 21, 58, 20, DateTimeKind.Utc).AddTicks(5180));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 18, 21, 58, 20, DateTimeKind.Utc).AddTicks(5187));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 18, 21, 58, 20, DateTimeKind.Utc).AddTicks(5192));

            migrationBuilder.CreateIndex(
                name: "IX_WorkerProfiles_NationalityId",
                table: "WorkerProfiles",
                column: "NationalityId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerProfiles_Countries_NationalityId",
                table: "WorkerProfiles",
                column: "NationalityId",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerProfiles_Countries_NationalityId",
                table: "WorkerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_WorkerProfiles_NationalityId",
                table: "WorkerProfiles");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 17, 53, 19, 253, DateTimeKind.Utc).AddTicks(1589));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 17, 53, 19, 253, DateTimeKind.Utc).AddTicks(1593));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 17, 53, 19, 253, DateTimeKind.Utc).AddTicks(1600));
        }
    }
}
