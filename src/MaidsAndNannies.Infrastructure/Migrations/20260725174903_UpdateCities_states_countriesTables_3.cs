using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCities_states_countriesTables_3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_Country_id",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_States_State_id",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_States_Countries_Country_id",
                table: "States");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 25, 17, 49, 2, 977, DateTimeKind.Utc).AddTicks(6255));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 25, 17, 49, 2, 977, DateTimeKind.Utc).AddTicks(6260));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 25, 17, 49, 2, 977, DateTimeKind.Utc).AddTicks(6264));

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_Country_id",
                table: "Cities",
                column: "Country_id",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_States_State_id",
                table: "Cities",
                column: "State_id",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_States_Countries_Country_id",
                table: "States",
                column: "Country_id",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Countries_Country_id",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Cities_States_State_id",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_States_Countries_Country_id",
                table: "States");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 25, 17, 39, 49, 289, DateTimeKind.Utc).AddTicks(6430));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 25, 17, 39, 49, 289, DateTimeKind.Utc).AddTicks(6435));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 25, 17, 39, 49, 289, DateTimeKind.Utc).AddTicks(6438));

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Countries_Country_id",
                table: "Cities",
                column: "Country_id",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_States_State_id",
                table: "Cities",
                column: "State_id",
                principalTable: "States",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_States_Countries_Country_id",
                table: "States",
                column: "Country_id",
                principalTable: "Countries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
