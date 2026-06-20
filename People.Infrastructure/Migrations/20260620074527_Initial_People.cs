using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_People : Migration
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
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "legalPersons",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    fkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RegisterCode = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_legalPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_legalPersons_Parties_fkPartyId",
                        column: x => x.fkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "naturalPerson",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    fkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_naturalPerson", x => x.Id);
                    table.ForeignKey(
                        name: "FK_naturalPerson_Parties_fkPartyId",
                        column: x => x.fkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonContacts",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonContacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonContacts_naturalPerson_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "people",
                        principalTable: "naturalPerson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PersonProfiles",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: true),
                    Enablity = table.Column<bool>(type: "bit", nullable: true),
                    Visiblity = table.Column<bool>(type: "bit", nullable: true),
                    Remove = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PersonProfiles_naturalPerson_FkPersonId",
                        column: x => x.FkPersonId,
                        principalSchema: "people",
                        principalTable: "naturalPerson",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_CreatedAt",
                schema: "people",
                table: "legalPersons",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_CreatedBy",
                schema: "people",
                table: "legalPersons",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_fkPartyId",
                schema: "people",
                table: "legalPersons",
                column: "fkPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_ModifiedAt",
                schema: "people",
                table: "legalPersons",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_legalPersons_ModifiedBy",
                schema: "people",
                table: "legalPersons",
                column: "ModifiedBy");

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
                filter: "[RegisterCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPerson_CreatedAt",
                schema: "people",
                table: "naturalPerson",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPerson_CreatedBy",
                schema: "people",
                table: "naturalPerson",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPerson_fkPartyId",
                schema: "people",
                table: "naturalPerson",
                column: "fkPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPerson_ModifiedAt",
                schema: "people",
                table: "naturalPerson",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPerson_ModifiedBy",
                schema: "people",
                table: "naturalPerson",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FullName_FastLookup",
                schema: "people",
                table: "naturalPerson",
                columns: new[] { "FirstName", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_Unique_NationalCode",
                schema: "people",
                table: "naturalPerson",
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
                name: "IX_Parties_CreatedAt",
                schema: "people",
                table: "Parties",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_CreatedBy",
                schema: "people",
                table: "Parties",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_ModifiedAt",
                schema: "people",
                table: "Parties",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_ModifiedBy",
                schema: "people",
                table: "Parties",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_CreatedAt",
                schema: "people",
                table: "PersonContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_CreatedBy",
                schema: "people",
                table: "PersonContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_ModifiedAt",
                schema: "people",
                table: "PersonContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_ModifiedBy",
                schema: "people",
                table: "PersonContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerOrgUnit",
                schema: "people",
                table: "PersonContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerPerson",
                schema: "people",
                table: "PersonContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_ScopedLookup",
                schema: "people",
                table: "PersonContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_PersonId",
                schema: "people",
                table: "PersonContacts",
                column: "PersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_CreatedAt",
                schema: "people",
                table: "PersonProfiles",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_CreatedBy",
                schema: "people",
                table: "PersonProfiles",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_ModifiedAt",
                schema: "people",
                table: "PersonProfiles",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_ModifiedBy",
                schema: "people",
                table: "PersonProfiles",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_OwnerOrgUnit",
                schema: "people",
                table: "PersonProfiles",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_OwnerPerson",
                schema: "people",
                table: "PersonProfiles",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfile_ScopedLookup",
                schema: "people",
                table: "PersonProfiles",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonProfiles_FkPersonId",
                schema: "people",
                table: "PersonProfiles",
                column: "FkPersonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "legalPersons",
                schema: "people");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "people");

            migrationBuilder.DropTable(
                name: "PersonContacts",
                schema: "people");

            migrationBuilder.DropTable(
                name: "PersonProfiles",
                schema: "people");

            migrationBuilder.DropTable(
                name: "naturalPerson",
                schema: "people");

            migrationBuilder.DropTable(
                name: "Parties",
                schema: "people");
        }
    }
}
