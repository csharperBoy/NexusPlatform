using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_9_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
