using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyIdColumnInJoppostsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrencyId",
                table: "JobPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 4, 57, 245, DateTimeKind.Utc).AddTicks(1517));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 4, 57, 245, DateTimeKind.Utc).AddTicks(1526));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 15, 4, 57, 245, DateTimeKind.Utc).AddTicks(1532));

            migrationBuilder.CreateIndex(
                name: "IX_JobPosts_CurrencyId",
                table: "JobPosts",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobPosts_Currencies_CurrencyId",
                table: "JobPosts",
                column: "CurrencyId",
                principalTable: "Currencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobPosts_Currencies_CurrencyId",
                table: "JobPosts");

            migrationBuilder.DropIndex(
                name: "IX_JobPosts_CurrencyId",
                table: "JobPosts");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "JobPosts");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 14, 24, 56, 276, DateTimeKind.Utc).AddTicks(219));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 14, 24, 56, 276, DateTimeKind.Utc).AddTicks(230));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 26, 14, 24, 56, 276, DateTimeKind.Utc).AddTicks(241));
        }
    }
}
