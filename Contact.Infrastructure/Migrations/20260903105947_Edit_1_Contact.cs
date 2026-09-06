using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contact");

            migrationBuilder.CreateTable(
                name: "ContactProfiles",
                schema: "contact",
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
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ProfileType = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactResources",
                schema: "contact",
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
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    ParentContactResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelationType = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactResources_ContactResources_ParentContactResourceId",
                        column: x => x.ParentContactResourceId,
                        principalSchema: "contact",
                        principalTable: "ContactResources",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "contact",
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
                name: "ContactProfileAssignments",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ContactProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactProfileAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactItems_ContactProfiles",
                        column: x => x.ContactProfileId,
                        principalSchema: "contact",
                        principalTable: "ContactProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContactResources_ContactProfiles",
                        column: x => x.ContactResourceId,
                        principalSchema: "contact",
                        principalTable: "ContactResources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignment_CreatedAt",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignment_CreatedBy",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignment_ModifiedAt",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignment_ModifiedBy",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignments_ContactProfileId",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "ContactProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignments_ContactResourceId",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "ContactResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfileAssignments_Id",
                schema: "contact",
                table: "ContactProfileAssignments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_CreatedAt",
                schema: "contact",
                table: "ContactProfiles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_CreatedBy",
                schema: "contact",
                table: "ContactProfiles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_ModifiedAt",
                schema: "contact",
                table: "ContactProfiles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_ModifiedBy",
                schema: "contact",
                table: "ContactProfiles",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_OwnerOrgUnit",
                schema: "contact",
                table: "ContactProfiles",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_OwnerPerson",
                schema: "contact",
                table: "ContactProfiles",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfile_ScopedLookup",
                schema: "contact",
                table: "ContactProfiles",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactProfiles_Id",
                schema: "contact",
                table: "ContactProfiles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_CreatedAt",
                schema: "contact",
                table: "ContactResources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_CreatedBy",
                schema: "contact",
                table: "ContactResources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_ModifiedAt",
                schema: "contact",
                table: "ContactResources",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_ModifiedBy",
                schema: "contact",
                table: "ContactResources",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_OwnerOrgUnit",
                schema: "contact",
                table: "ContactResources",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_OwnerPerson",
                schema: "contact",
                table: "ContactResources",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactResource_ScopedLookup",
                schema: "contact",
                table: "ContactResources",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactResources_Id",
                schema: "contact",
                table: "ContactResources",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactResources_ParentContactResourceId",
                schema: "contact",
                table: "ContactResources",
                column: "ParentContactResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "contact",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "contact",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "contact",
                table: "OutboxMessages",
                column: "TypeName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactProfileAssignments",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "ContactProfiles",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "ContactResources",
                schema: "contact");
        }
    }
}
