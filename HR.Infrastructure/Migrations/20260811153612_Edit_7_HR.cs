using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_7_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationContacts",
                schema: "hr",
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
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationContacts_Locations",
                        column: x => x.FkLocationId,
                        principalSchema: "hr",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostLocations",
                schema: "hr",
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
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostLocations_Location",
                        column: x => x.FkLocationId,
                        principalSchema: "hr",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostLocations_Post",
                        column: x => x.FkPostId,
                        principalSchema: "hr",
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_CreatedAt",
                schema: "hr",
                table: "LocationContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_CreatedBy",
                schema: "hr",
                table: "LocationContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ModifiedAt",
                schema: "hr",
                table: "LocationContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ModifiedBy",
                schema: "hr",
                table: "LocationContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_OwnerOrgUnit",
                schema: "hr",
                table: "LocationContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_OwnerPerson",
                schema: "hr",
                table: "LocationContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ScopedLookup",
                schema: "hr",
                table: "LocationContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationContacts_FkLocationId",
                schema: "hr",
                table: "LocationContacts",
                column: "FkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContacts_Id",
                schema: "hr",
                table: "LocationContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PostLocation_CreatedAt",
                schema: "hr",
                table: "PostLocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocation_CreatedBy",
                schema: "hr",
                table: "PostLocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocation_ModifiedAt",
                schema: "hr",
                table: "PostLocations",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocation_ModifiedBy",
                schema: "hr",
                table: "PostLocations",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_fkLocationId",
                schema: "hr",
                table: "PostLocations",
                column: "FkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_fkPostId",
                schema: "hr",
                table: "PostLocations",
                column: "FkPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_Id",
                schema: "hr",
                table: "PostLocations",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationContacts",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "PostLocations",
                schema: "hr");
        }
    }
}
