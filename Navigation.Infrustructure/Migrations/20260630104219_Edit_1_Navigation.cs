using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navigation.Infrustructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_Navigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "navigation");

            migrationBuilder.CreateTable(
                name: "Menu",
                schema: "navigation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Key = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Menu", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Menu_Menu_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "navigation",
                        principalTable: "Menu",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "navigation",
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

            migrationBuilder.CreateIndex(
                name: "IX_Menu_CreatedAt",
                schema: "navigation",
                table: "Menu",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_CreatedBy",
                schema: "navigation",
                table: "Menu",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Id",
                schema: "navigation",
                table: "Menu",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menu_Key",
                schema: "navigation",
                table: "Menu",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Menu_ModifiedAt",
                schema: "navigation",
                table: "Menu",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_ModifiedBy",
                schema: "navigation",
                table: "Menu",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_OwnerOrgUnit",
                schema: "navigation",
                table: "Menu",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_OwnerPerson",
                schema: "navigation",
                table: "Menu",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_ParentId",
                schema: "navigation",
                table: "Menu",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Menu_ScopedLookup",
                schema: "navigation",
                table: "Menu",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "navigation",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "navigation",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "navigation",
                table: "OutboxMessages",
                column: "TypeName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Menu",
                schema: "navigation");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "navigation");
        }
    }
}
