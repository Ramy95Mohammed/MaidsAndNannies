using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MaidsAndNannies.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerSpecializationSpec_WorkerProfiles_WorkerProfileId",
                table: "WorkerSpecializationSpec");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkerSpecializationSpec",
                table: "WorkerSpecializationSpec");

            migrationBuilder.RenameTable(
                name: "WorkerSpecializationSpec",
                newName: "WorkerSpecializationSpecs");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerSpecializationSpec_WorkerProfileId",
                table: "WorkerSpecializationSpecs",
                newName: "IX_WorkerSpecializationSpecs_WorkerProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkerSpecializationSpecs",
                table: "WorkerSpecializationSpecs",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerSpecializationSpecs_WorkerProfiles_WorkerProfileId",
                table: "WorkerSpecializationSpecs",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkerSpecializationSpecs_WorkerProfiles_WorkerProfileId",
                table: "WorkerSpecializationSpecs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkerSpecializationSpecs",
                table: "WorkerSpecializationSpecs");

            migrationBuilder.RenameTable(
                name: "WorkerSpecializationSpecs",
                newName: "WorkerSpecializationSpec");

            migrationBuilder.RenameIndex(
                name: "IX_WorkerSpecializationSpecs_WorkerProfileId",
                table: "WorkerSpecializationSpec",
                newName: "IX_WorkerSpecializationSpec_WorkerProfileId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkerSpecializationSpec",
                table: "WorkerSpecializationSpec",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkerSpecializationSpec_WorkerProfiles_WorkerProfileId",
                table: "WorkerSpecializationSpec",
                column: "WorkerProfileId",
                principalTable: "WorkerProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
