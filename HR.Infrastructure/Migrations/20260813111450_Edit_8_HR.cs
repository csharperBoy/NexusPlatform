using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_8_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ EmploymentLocations_ Employment",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_ EmploymentLocations_Location",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLocations_Location",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLocations_Post",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropTable(
                name: "EmploymentContacts",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "LocationContacts",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "PostContacts",
                schema: "hr");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostLocation",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocation_CreatedAt",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocation_CreatedBy",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocation_ModifiedAt",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocation_ModifiedBy",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocations_fkLocationId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocations_fkPostId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocations_Id",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploymentLocation",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_ EmploymentLocations_fkEmploymentId",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_ EmploymentLocations_fkLocationId",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_ EmploymentLocations_Id",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentLocation_CreatedAt",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentLocation_CreatedBy",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentLocation_ModifiedAt",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentLocation_ModifiedBy",
                schema: "hr",
                table: " EmploymentLocations");

            migrationBuilder.RenameTable(
                name: " EmploymentLocations",
                schema: "hr",
                newName: "EmploymentLocations",
                newSchema: "hr");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "hr",
                table: "PostLocations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "hr",
                table: "PostLocations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: "PostLocations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "hr",
                table: "PostLocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PostId",
                schema: "hr",
                table: "PostLocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "hr",
                table: "EmploymentLocations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "hr",
                table: "EmploymentLocations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: "EmploymentLocations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<Guid>(
                name: "EmploymentId",
                schema: "hr",
                table: "EmploymentLocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                schema: "hr",
                table: "EmploymentLocations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostLocations",
                schema: "hr",
                table: "PostLocations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploymentLocations",
                schema: "hr",
                table: "EmploymentLocations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_LocationId",
                schema: "hr",
                table: "PostLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_PostId",
                schema: "hr",
                table: "PostLocations",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_EmploymentId",
                schema: "hr",
                table: "EmploymentLocations",
                column: "EmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_LocationId",
                schema: "hr",
                table: "EmploymentLocations",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentLocations_ Employment_EmploymentId",
                schema: "hr",
                table: "EmploymentLocations",
                column: "EmploymentId",
                principalSchema: "hr",
                principalTable: " Employment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmploymentLocations_Location_LocationId",
                schema: "hr",
                table: "EmploymentLocations",
                column: "LocationId",
                principalSchema: "hr",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLocations_Location_LocationId",
                schema: "hr",
                table: "PostLocations",
                column: "LocationId",
                principalSchema: "hr",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLocations_Post_PostId",
                schema: "hr",
                table: "PostLocations",
                column: "PostId",
                principalSchema: "hr",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentLocations_ Employment_EmploymentId",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_EmploymentLocations_Location_LocationId",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLocations_Location_LocationId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropForeignKey(
                name: "FK_PostLocations_Post_PostId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PostLocations",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocations_LocationId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropIndex(
                name: "IX_PostLocations_PostId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EmploymentLocations",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentLocations_EmploymentId",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropIndex(
                name: "IX_EmploymentLocations_LocationId",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropColumn(
                name: "PostId",
                schema: "hr",
                table: "PostLocations");

            migrationBuilder.DropColumn(
                name: "EmploymentId",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                schema: "hr",
                table: "EmploymentLocations");

            migrationBuilder.RenameTable(
                name: "EmploymentLocations",
                schema: "hr",
                newName: " EmploymentLocations",
                newSchema: "hr");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "hr",
                table: "PostLocations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "hr",
                table: "PostLocations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: "PostLocations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "ModifiedBy",
                schema: "hr",
                table: " EmploymentLocations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "hr",
                table: " EmploymentLocations",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "hr",
                table: " EmploymentLocations",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PostLocation",
                schema: "hr",
                table: "PostLocations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EmploymentLocation",
                schema: "hr",
                table: " EmploymentLocations",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "EmploymentContacts",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_EmploymentContact", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmploymentContacts_Employments",
                        column: x => x.FkEmploymentId,
                        principalSchema: "hr",
                        principalTable: " Employment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationContacts",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                name: "PostContacts",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_fkEmploymentId",
                schema: "hr",
                table: " EmploymentLocations",
                column: "FkEmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_fkLocationId",
                schema: "hr",
                table: " EmploymentLocations",
                column: "FkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_Id",
                schema: "hr",
                table: " EmploymentLocations",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_CreatedAt",
                schema: "hr",
                table: " EmploymentLocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_CreatedBy",
                schema: "hr",
                table: " EmploymentLocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_ModifiedAt",
                schema: "hr",
                table: " EmploymentLocations",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_ModifiedBy",
                schema: "hr",
                table: " EmploymentLocations",
                column: "ModifiedBy");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ EmploymentLocations_ Employment",
                schema: "hr",
                table: " EmploymentLocations",
                column: "FkEmploymentId",
                principalSchema: "hr",
                principalTable: " Employment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ EmploymentLocations_Location",
                schema: "hr",
                table: " EmploymentLocations",
                column: "FkLocationId",
                principalSchema: "hr",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLocations_Location",
                schema: "hr",
                table: "PostLocations",
                column: "FkLocationId",
                principalSchema: "hr",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PostLocations_Post",
                schema: "hr",
                table: "PostLocations",
                column: "FkPostId",
                principalSchema: "hr",
                principalTable: "Post",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
