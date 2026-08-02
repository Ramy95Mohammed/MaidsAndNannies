using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyQuantityAndSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.InsertData(
    table: "AppSettings",
    columns: new[] { "Key", "Description", "UpdatedAt", "UpdatedBy", "Value" },
    values: new object[] { "MonthlyWorkingDaysPerMonth", "عدد أيام العمل القياسية في الشهر لحساب الأجر الشهري النسبي", DateTime.UtcNow, "system", "26" });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 8, 2, 18, 17, 25, 704, DateTimeKind.Utc).AddTicks(8446));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 8, 2, 18, 17, 25, 704, DateTimeKind.Utc).AddTicks(8458));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 8, 2, 18, 17, 25, 704, DateTimeKind.Utc).AddTicks(8465));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 8, 2, 17, 18, 49, 600, DateTimeKind.Utc).AddTicks(3050));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 8, 2, 17, 18, 49, 600, DateTimeKind.Utc).AddTicks(3061));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 8, 2, 17, 18, 49, 600, DateTimeKind.Utc).AddTicks(3067));
        }
    }
}
