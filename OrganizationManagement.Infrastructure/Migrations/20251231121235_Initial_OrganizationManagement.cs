using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_OrganizationManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "OrganizationManagement");

            migrationBuilder.EnsureSchema(
                name: "organization");

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Path = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AssemblyQualifiedName = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErrorStackTrace = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    EventVersion = table.Column<int>(type: "int", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Positions",
                schema: "organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportsToPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsManagerial = table.Column<bool>(type: "bit", nullable: false),
                    ReportsToId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Positions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Positions_OrganizationUnits_FkOrganizationUnitId",
                        column: x => x.FkOrganizationUnitId,
                        principalSchema: "organization",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Positions_Positions_ReportsToId",
                        column: x => x.ReportsToId,
                        principalSchema: "organization",
                        principalTable: "Positions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "OrganizationManagement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_Positions_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "organization",
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_CreatedAt",
                schema: "OrganizationManagement",
                table: "Assignments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_CreatedBy",
                schema: "OrganizationManagement",
                table: "Assignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ModifiedAt",
                schema: "OrganizationManagement",
                table: "Assignments",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ModifiedBy",
                schema: "OrganizationManagement",
                table: "Assignments",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Id",
                schema: "OrganizationManagement",
                table: "Assignments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_PositionId",
                schema: "OrganizationManagement",
                table: "Assignments",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_CreatedAt",
                schema: "organization",
                table: "OrganizationUnits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_CreatedBy",
                schema: "organization",
                table: "OrganizationUnits",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ModifiedAt",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ModifiedBy",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Code",
                schema: "organization",
                table: "OrganizationUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ParentId",
                schema: "organization",
                table: "OrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Path",
                schema: "organization",
                table: "OrganizationUnits",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "organization",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "organization",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "organization",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_Position_CreatedAt",
                schema: "organization",
                table: "Positions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Position_CreatedBy",
                schema: "organization",
                table: "Positions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Position_ModifiedAt",
                schema: "organization",
                table: "Positions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Position_ModifiedBy",
                schema: "organization",
                table: "Positions",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_FkOrganizationUnitId",
                schema: "organization",
                table: "Positions",
                column: "FkOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Positions_ReportsToId",
                schema: "organization",
                table: "Positions",
                column: "ReportsToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignments",
                schema: "OrganizationManagement");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "organization");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "organization");
        }
    }
}
