using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.IrisaSync.Extention.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_IrisaSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.CreateTable(
                name: "IrisaSyncJobLevelMap",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkJobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IrisaJobLevelId = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IrisaJobLevel = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrisaSyncJobLevelMap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IrisaSyncJobTitleMap",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkJobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IrisaJobTitleId = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IrisaJobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IrisaJobTitleUseCount = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrisaSyncJobTitleMap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IrisaSyncOrganizationUnitMap",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IrisaOrganizationUnitId = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IrisaOrganizationUnit = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrisaSyncOrganizationUnitMap", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_JobLevelId",
                schema: "hr",
                table: "IrisaSyncJobLevelMap",
                column: "FkJobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_FkJobLevelId_Unique",
                schema: "hr",
                table: "IrisaSyncJobLevelMap",
                column: "FkJobLevelId",
                unique: true,
                filter: "[FkJobLevelId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaJobLevelId_Unique",
                schema: "hr",
                table: "IrisaSyncJobLevelMap",
                column: "IrisaJobLevelId",
                unique: true,
                filter: "[IrisaJobLevelId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaSyncJobLevelMap_Id",
                schema: "hr",
                table: "IrisaSyncJobLevelMap",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_JobTitleId",
                schema: "hr",
                table: "IrisaSyncJobTitleMap",
                column: "FkJobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_FkJobTitleId_Unique",
                schema: "hr",
                table: "IrisaSyncJobTitleMap",
                column: "FkJobTitleId",
                unique: true,
                filter: "[FkJobTitleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaJobTitleId_Unique",
                schema: "hr",
                table: "IrisaSyncJobTitleMap",
                column: "IrisaJobTitleId",
                unique: true,
                filter: "[IrisaJobTitleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaSyncJobTitleMap_Id",
                schema: "hr",
                table: "IrisaSyncJobTitleMap",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_OrganizationUnitId",
                schema: "hr",
                table: "IrisaSyncOrganizationUnitMap",
                column: "FkOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_FkOrganizationUnitId_Unique",
                schema: "hr",
                table: "IrisaSyncOrganizationUnitMap",
                column: "FkOrganizationUnitId",
                unique: true,
                filter: "[FkOrganizationUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaOrganizationUnitId_Unique",
                schema: "hr",
                table: "IrisaSyncOrganizationUnitMap",
                column: "IrisaOrganizationUnitId",
                unique: true,
                filter: "[IrisaOrganizationUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaSyncOrganizationUnitMap_Id",
                schema: "hr",
                table: "IrisaSyncOrganizationUnitMap",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IrisaSyncJobLevelMap",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "IrisaSyncJobTitleMap",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "IrisaSyncOrganizationUnitMap",
                schema: "hr");
        }
    }
}
