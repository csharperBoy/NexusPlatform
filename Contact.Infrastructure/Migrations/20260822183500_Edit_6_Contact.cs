using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_6_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactItems",
                schema: "contact");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactProfileAssignments",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "ContactResources",
                schema: "contact");

            migrationBuilder.CreateTable(
                name: "ContactItems",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentContactItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelationType = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContactItems_ContactItems_ParentContactItemId",
                        column: x => x.ParentContactItemId,
                        principalSchema: "contact",
                        principalTable: "ContactItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContactItems_ContactProfiles",
                        column: x => x.ContactProfileId,
                        principalSchema: "contact",
                        principalTable: "ContactProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_CreatedAt",
                schema: "contact",
                table: "ContactItems",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_CreatedBy",
                schema: "contact",
                table: "ContactItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_ModifiedAt",
                schema: "contact",
                table: "ContactItems",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_ModifiedBy",
                schema: "contact",
                table: "ContactItems",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_OwnerOrgUnit",
                schema: "contact",
                table: "ContactItems",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_OwnerPerson",
                schema: "contact",
                table: "ContactItems",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItem_ScopedLookup",
                schema: "contact",
                table: "ContactItems",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContactItems_ContactProfileId",
                schema: "contact",
                table: "ContactItems",
                column: "ContactProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ContactItems_Id",
                schema: "contact",
                table: "ContactItems",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactItems_ParentContactItemId",
                schema: "contact",
                table: "ContactItems",
                column: "ParentContactItemId");
        }
    }
}
