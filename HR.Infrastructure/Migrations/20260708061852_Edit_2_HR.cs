using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_2_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PostContacts",
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
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostContacts_Posts",
                        column: x => x.FkPostId,
                        principalSchema: "hr",
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkPostId",
                schema: "hr",
                table: "PostContacts",
                column: "FkPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_CreatedAt",
                schema: "hr",
                table: "PostContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_CreatedBy",
                schema: "hr",
                table: "PostContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ModifiedAt",
                schema: "hr",
                table: "PostContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ModifiedBy",
                schema: "hr",
                table: "PostContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_OwnerOrgUnit",
                schema: "hr",
                table: "PostContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_OwnerPerson",
                schema: "hr",
                table: "PostContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ScopedLookup",
                schema: "hr",
                table: "PostContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PostContacts_Id",
                schema: "hr",
                table: "PostContacts",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PostContacts",
                schema: "hr");
        }
    }
}
