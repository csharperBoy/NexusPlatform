using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_2_People : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartyContacts",
                schema: "people");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PartyContacts",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_PartyContacts_Parties",
                        column: x => x.FkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
    }
}
