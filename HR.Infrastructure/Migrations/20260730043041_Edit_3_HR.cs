using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_3_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmploymentContacts",
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
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmploymentContacts_Employments",
                        column: x => x.FkEmploymentId,
                        principalSchema: "hr",
                        principalTable: " Employment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_CreatedAt",
                schema: "hr",
                table: "EmploymentContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_CreatedBy",
                schema: "hr",
                table: "EmploymentContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ModifiedAt",
                schema: "hr",
                table: "EmploymentContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ModifiedBy",
                schema: "hr",
                table: "EmploymentContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_OwnerOrgUnit",
                schema: "hr",
                table: "EmploymentContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_OwnerPerson",
                schema: "hr",
                table: "EmploymentContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ScopedLookup",
                schema: "hr",
                table: "EmploymentContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContacts_Id",
                schema: "hr",
                table: "EmploymentContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkEmploymentId",
                schema: "hr",
                table: "EmploymentContacts",
                column: "FkEmploymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmploymentContacts",
                schema: "hr");
        }
    }
}
