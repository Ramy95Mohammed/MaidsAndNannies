using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyIdInBokkingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 18, 54, 7, 708, DateTimeKind.Utc).AddTicks(6223));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 18, 54, 7, 708, DateTimeKind.Utc).AddTicks(6246));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 18, 54, 7, 708, DateTimeKind.Utc).AddTicks(6252));

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_CurrencyId",
                table: "Bookings",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Currencies_CurrencyId",
                table: "Bookings",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Currencies_CurrencyId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_CurrencyId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "Bookings");

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
        }
    }
}
