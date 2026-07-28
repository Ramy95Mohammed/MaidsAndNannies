using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatchUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "MaxReplacementCount");

            migrationBuilder.AddColumn<int>(
                name: "LastReplacementReason",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReplacedFromBookingId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Key", "Description", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { "MaxFaultReplacementCount", "الحد الأقصى لعدد مرات الاستبدال بسبب تقصير العاملة", null, null, "3" },
                    { "MaxPreferenceReplacementCount", "الحد الأقصى لعدد مرات الاستبدال برغبة شخصية من صاحبة المنزل", null, null, "1" }
                });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 28, 2, 18, 3, 903, DateTimeKind.Utc).AddTicks(3225));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 28, 2, 18, 3, 903, DateTimeKind.Utc).AddTicks(3230));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 28, 2, 18, 3, 903, DateTimeKind.Utc).AddTicks(3234));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "MaxFaultReplacementCount");

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "MaxPreferenceReplacementCount");

            migrationBuilder.DropColumn(
                name: "LastReplacementReason",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "ReplacedFromBookingId",
                table: "Bookings");

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Key", "Description", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[] { "MaxReplacementCount", "الحد الأقصى لعدد مرات الاستبدال لكل حجز", null, null, "2" });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 27, 11, 26, 3, 978, DateTimeKind.Utc).AddTicks(9393));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 27, 11, 26, 3, 978, DateTimeKind.Utc).AddTicks(9399));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 27, 11, 26, 3, 978, DateTimeKind.Utc).AddTicks(9406));
        }
    }
}
