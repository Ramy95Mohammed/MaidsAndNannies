using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDailySalaryAndHourlySalaryColumnsInBookingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DailySalary",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlySalary",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 24, 16, 17, 40, 937, DateTimeKind.Utc).AddTicks(4114));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 24, 16, 17, 40, 937, DateTimeKind.Utc).AddTicks(4122));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 24, 16, 17, 40, 937, DateTimeKind.Utc).AddTicks(4129));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailySalary",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "HourlySalary",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 24, 14, 47, 17, 731, DateTimeKind.Utc).AddTicks(8085));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 24, 14, 47, 17, 731, DateTimeKind.Utc).AddTicks(8094));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 24, 14, 47, 17, 731, DateTimeKind.Utc).AddTicks(8101));
        }
    }
}
