using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_People : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "people");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "people",
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
                name: "Parties",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkPermissionAssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Party", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "legalPersons",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisterCode = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_legalPersons_Parties",
                        column: x => x.FkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "naturalPersons",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaturalPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_naturalPersons_Parties",
                        column: x => x.FkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartiesRelations",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkSourcePartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkDestinationPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RelationType = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartiesRelation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartiesRelations_Parties",
                        column: x => x.FkSourcePartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartiesRelations_Parties1",
                        column: x => x.FkDestinationPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartyContacts",
                schema: "people",
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
                    FkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartyContacts_Parties",
                        column: x => x.FkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NaturalPersonProfiles",
                schema: "people",
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
                    FkNaturalPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: true),
                    Enablity = table.Column<bool>(type: "bit", nullable: true),
                    Visiblity = table.Column<bool>(type: "bit", nullable: true),
                    Remove = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NaturalPersonProfile", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NaturalPersonProfiles_naturalPersons",
                        column: x => x.FkNaturalPersonId,
                        principalSchema: "people",
                        principalTable: "naturalPersons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalPerson_CreatedAt",
                schema: "people",
                table: "legalPersons",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LegalPerson_CreatedBy",
                schema: "people",
                table: "legalPersons",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LegalPerson_ModifiedAt",
                schema: "people",
                table: "legalPersons",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LegalPerson_ModifiedBy",
                schema: "people",
                table: "legalPersons",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_fkPartyId",
                schema: "people",
                table: "legalPersons",
                column: "FkPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_Id",
                schema: "people",
                table: "legalPersons",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FullName_FastLookup",
                schema: "people",
                table: "legalPersons",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Unique",
                schema: "people",
                table: "legalPersons",
                column: "RegisterCode",
                unique: true,
                filter: "([RegisterCode] IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_CreatedAt",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_CreatedBy",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_ModifiedAt",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_ModifiedBy",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_OwnerOrgUnit",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_OwnerPerson",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfile_ScopedLookup",
                schema: "people",
                table: "NaturalPersonProfiles",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPersonProfiles_Id",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_OwnerOrgUnit",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_OwnerPerson",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_ScopedLookup",
                schema: "people",
                table: "NaturalPersonProfiles",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfiles_FkPersonId",
                schema: "people",
                table: "NaturalPersonProfiles",
                column: "FkNaturalPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPerson_CreatedAt",
                schema: "people",
                table: "naturalPersons",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPerson_CreatedBy",
                schema: "people",
                table: "naturalPersons",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPerson_ModifiedAt",
                schema: "people",
                table: "naturalPersons",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NaturalPerson_ModifiedBy",
                schema: "people",
                table: "naturalPersons",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_fkPartyId",
                schema: "people",
                table: "naturalPersons",
                column: "FkPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_Id",
                schema: "people",
                table: "naturalPersons",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FullName_FastLookup",
                schema: "people",
                table: "naturalPersons",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Unique_NationalCode",
                schema: "people",
                table: "naturalPersons",
                column: "NationalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "people",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "people",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "people",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_Id",
                schema: "people",
                table: "Parties",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Party_CreatedAt",
                schema: "people",
                table: "Parties",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Party_CreatedBy",
                schema: "people",
                table: "Parties",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Party_ModifiedAt",
                schema: "people",
                table: "Parties",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Party_ModifiedBy",
                schema: "people",
                table: "Parties",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelation_CreatedAt",
                schema: "people",
                table: "PartiesRelations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelation_CreatedBy",
                schema: "people",
                table: "PartiesRelations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelation_ModifiedAt",
                schema: "people",
                table: "PartiesRelations",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelation_ModifiedBy",
                schema: "people",
                table: "PartiesRelations",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelations_destinationPartyId",
                schema: "people",
                table: "PartiesRelations",
                column: "FkDestinationPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelations_Id",
                schema: "people",
                table: "PartiesRelations",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartiesRelations_sourcePartyId",
                schema: "people",
                table: "PartiesRelations",
                column: "FkSourcePartyId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_CreatedAt",
                schema: "people",
                table: "PartyContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_CreatedBy",
                schema: "people",
                table: "PartyContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ModifiedAt",
                schema: "people",
                table: "PartyContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ModifiedBy",
                schema: "people",
                table: "PartyContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_OwnerOrgUnit",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_OwnerPerson",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ScopedLookup",
                schema: "people",
                table: "PartyContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartyContacts_Id",
                schema: "people",
                table: "PartyContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerOrgUnit",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerPerson",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_ScopedLookup",
                schema: "people",
                table: "PartyContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkPartyId",
                schema: "people",
                table: "PartyContacts",
                column: "FkPartyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "legalPersons",
                schema: "people");

            migrationBuilder.DropTable(
                name: "NaturalPersonProfiles",
                schema: "people");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "people");

            migrationBuilder.DropTable(
                name: "PartiesRelations",
                schema: "people");

            migrationBuilder.DropTable(
                name: "PartyContacts",
                schema: "people");

            migrationBuilder.DropTable(
                name: "naturalPersons",
                schema: "people");

            migrationBuilder.DropTable(
                name: "Parties",
                schema: "people");
        }
    }
}
