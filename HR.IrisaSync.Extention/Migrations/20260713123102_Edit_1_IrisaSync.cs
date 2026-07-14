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
                name: "JobLevelMap",
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
                    table.PrimaryKey("PK_JobLevelMap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTitleMap",
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
                    table.PrimaryKey("PK_JobTitleMap", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnitMap",
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
                    table.PrimaryKey("PK_OrganizationUnitMap", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_JobLevelId",
                schema: "hr",
                table: "JobLevelMap",
                column: "FkJobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_FkJobLevelId_Unique",
                schema: "hr",
                table: "JobLevelMap",
                column: "FkJobLevelId",
                unique: true,
                filter: "[FkJobLevelId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaJobLevelId_Unique",
                schema: "hr",
                table: "JobLevelMap",
                column: "IrisaJobLevelId",
                unique: true,
                filter: "[IrisaJobLevelId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JobLevelMap_Id",
                schema: "hr",
                table: "JobLevelMap",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_JobTitleId",
                schema: "hr",
                table: "JobTitleMap",
                column: "FkJobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_FkJobTitleId_Unique",
                schema: "hr",
                table: "JobTitleMap",
                column: "FkJobTitleId",
                unique: true,
                filter: "[FkJobTitleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaJobTitleId_Unique",
                schema: "hr",
                table: "JobTitleMap",
                column: "IrisaJobTitleId",
                unique: true,
                filter: "[IrisaJobTitleId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_JobTitleMap_Id",
                schema: "hr",
                table: "JobTitleMap",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_OrganizationUnitId",
                schema: "hr",
                table: "OrganizationUnitMap",
                column: "FkOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_FkOrganizationUnitId_Unique",
                schema: "hr",
                table: "OrganizationUnitMap",
                column: "FkOrganizationUnitId",
                unique: true,
                filter: "[FkOrganizationUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IrisaOrganizationUnitId_Unique",
                schema: "hr",
                table: "OrganizationUnitMap",
                column: "IrisaOrganizationUnitId",
                unique: true,
                filter: "[IrisaOrganizationUnitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnitMap_Id",
                schema: "hr",
                table: "OrganizationUnitMap",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobLevelMap",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "JobTitleMap",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "OrganizationUnitMap",
                schema: "hr");
        }
    }
}
