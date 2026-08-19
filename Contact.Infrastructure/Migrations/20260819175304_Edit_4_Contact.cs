using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_4_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmploymentContacts",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "LocationContacts",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "PartyContacts",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "PostContacts",
                schema: "contact");

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
                name: "ContactItems",
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
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ContactProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Label = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    ParentContactItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelationType = table.Column<int>(type: "int", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactItems",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "ContactProfiles",
                schema: "contact");

            migrationBuilder.CreateTable(
                name: "EmploymentContacts",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationContacts",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartyContacts",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostContacts",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostContact", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_CreatedAt",
                schema: "contact",
                table: "EmploymentContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_CreatedBy",
                schema: "contact",
                table: "EmploymentContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ModifiedAt",
                schema: "contact",
                table: "EmploymentContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ModifiedBy",
                schema: "contact",
                table: "EmploymentContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_OwnerOrgUnit",
                schema: "contact",
                table: "EmploymentContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_OwnerPerson",
                schema: "contact",
                table: "EmploymentContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ScopedLookup",
                schema: "contact",
                table: "EmploymentContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContacts_Id",
                schema: "contact",
                table: "EmploymentContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkEmploymentId",
                schema: "contact",
                table: "EmploymentContacts",
                column: "FkEmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_CreatedAt",
                schema: "contact",
                table: "LocationContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_CreatedBy",
                schema: "contact",
                table: "LocationContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ModifiedAt",
                schema: "contact",
                table: "LocationContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ModifiedBy",
                schema: "contact",
                table: "LocationContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_OwnerOrgUnit",
                schema: "contact",
                table: "LocationContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_OwnerPerson",
                schema: "contact",
                table: "LocationContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ScopedLookup",
                schema: "contact",
                table: "LocationContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationContacts_FkLocationId",
                schema: "contact",
                table: "LocationContacts",
                column: "FkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContacts_Id",
                schema: "contact",
                table: "LocationContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_CreatedAt",
                schema: "contact",
                table: "PartyContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_CreatedBy",
                schema: "contact",
                table: "PartyContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ModifiedAt",
                schema: "contact",
                table: "PartyContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ModifiedBy",
                schema: "contact",
                table: "PartyContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_OwnerOrgUnit",
                schema: "contact",
                table: "PartyContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_OwnerPerson",
                schema: "contact",
                table: "PartyContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ScopedLookup",
                schema: "contact",
                table: "PartyContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartyContacts_Id",
                schema: "contact",
                table: "PartyContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerOrgUnit",
                schema: "contact",
                table: "PartyContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerPerson",
                schema: "contact",
                table: "PartyContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_ScopedLookup",
                schema: "contact",
                table: "PartyContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkPartyId",
                schema: "contact",
                table: "PartyContacts",
                column: "FkPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkPostId",
                schema: "contact",
                table: "PostContacts",
                column: "FkPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_CreatedAt",
                schema: "contact",
                table: "PostContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_CreatedBy",
                schema: "contact",
                table: "PostContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ModifiedAt",
                schema: "contact",
                table: "PostContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ModifiedBy",
                schema: "contact",
                table: "PostContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_OwnerOrgUnit",
                schema: "contact",
                table: "PostContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_OwnerPerson",
                schema: "contact",
                table: "PostContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ScopedLookup",
                schema: "contact",
                table: "PostContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PostContacts_Id",
                schema: "contact",
                table: "PostContacts",
                column: "Id",
                unique: true);
        }
    }
}
