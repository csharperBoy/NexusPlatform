using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit3_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Location",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_Location_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "hr",
                        principalTable: "Location",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: " EmploymentLocations",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    fkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fkEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ EmploymentLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ EmploymentLocations_ Employment_fkEmployeeId",
                        column: x => x.fkEmployeeId,
                        principalSchema: "hr",
                        principalTable: " Employment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ EmploymentLocations_Location_fkLocationId",
                        column: x => x.fkLocationId,
                        principalSchema: "hr",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_fkEmployeeId",
                schema: "hr",
                table: " EmploymentLocations",
                column: "fkEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_fkLocationId",
                schema: "hr",
                table: " EmploymentLocations",
                column: "fkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_CreatedAt",
                schema: "hr",
                table: " EmploymentLocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_CreatedBy",
                schema: "hr",
                table: " EmploymentLocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_ModifiedAt",
                schema: "hr",
                table: " EmploymentLocations",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_ModifiedBy",
                schema: "hr",
                table: " EmploymentLocations",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Location_CreatedAt",
                schema: "hr",
                table: "Location",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Location_CreatedBy",
                schema: "hr",
                table: "Location",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ModifiedAt",
                schema: "hr",
                table: "Location",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ModifiedBy",
                schema: "hr",
                table: "Location",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ParentId",
                schema: "hr",
                table: "Location",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: " EmploymentLocations",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "hr");
        }
    }
}
