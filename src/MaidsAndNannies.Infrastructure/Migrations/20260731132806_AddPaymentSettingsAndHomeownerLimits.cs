using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSettingsAndHomeownerLimits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxFaultReplacementCount",
                table: "HomeownerProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxPreferenceReplacementCount",
                table: "HomeownerProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Key", "Description", "UpdatedAt", "UpdatedBy", "Value" },
                values: new object[,]
                {
                    { "CommissionBillingMode", "المبلغ المطلوب من صاحبة المنزل عند الدفع: CommissionOnly = العمولة فقط، CommissionPlusSalary = العمولة + مرتب العاملة", null, null, "CommissionOnly" },
                    { "RequirePaymentProof", "إظهار قسم رفع إثبات الدفع: true = ترفع صاحبة المنزل إثبات الدفع، false = يُعتبر الحجز مدفوعاً فور طلب الدفع (التواصل عبر واتساب)", null, null, "true" }
                });

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 31, 13, 28, 1, 12, DateTimeKind.Utc).AddTicks(4985));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 31, 13, 28, 1, 12, DateTimeKind.Utc).AddTicks(5001));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 31, 13, 28, 1, 12, DateTimeKind.Utc).AddTicks(5008));

            migrationBuilder.CreateIndex(
                name: "IX_WorkerProfiles_CountryId",
                table: "WorkerProfiles",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkerProfiles_StateId",
                table: "WorkerProfiles",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerProfiles_Countries_CountryId",
                table: "WorkerProfiles",
                column: "CountryId",
                principalTable: "Countries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerProfiles_States_StateId",
                table: "WorkerProfiles",
                column: "StateId",
                principalTable: "States",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerProfiles_Countries_CountryId",
                table: "WorkerProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkerProfiles_States_StateId",
                table: "WorkerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_WorkerProfiles_CountryId",
                table: "WorkerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_WorkerProfiles_StateId",
                table: "WorkerProfiles");

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "CommissionBillingMode");

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Key",
                keyValue: "RequirePaymentProof");

            migrationBuilder.DropColumn(
                name: "MaxFaultReplacementCount",
                table: "HomeownerProfiles");

            migrationBuilder.DropColumn(
                name: "MaxPreferenceReplacementCount",
                table: "HomeownerProfiles");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 29, 13, 10, 30, 358, DateTimeKind.Utc).AddTicks(5943));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 29, 13, 10, 30, 358, DateTimeKind.Utc).AddTicks(5951));

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedAtUtc",
                value: new DateTime(2026, 7, 29, 13, 10, 30, 358, DateTimeKind.Utc).AddTicks(5956));
        }
    }
}
