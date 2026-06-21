using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace People.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit2_People : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_naturalPerson_PersonId",
                schema: "people",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonProfiles_naturalPerson_FkPersonId",
                schema: "people",
                table: "PersonProfiles");

            migrationBuilder.DropTable(
                name: "naturalPerson",
                schema: "people");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "PersonProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "PersonContacts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "Parties",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "legalPersons",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

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
                    fkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_naturalPersons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_naturalPersons_Parties_fkPartyId",
                        column: x => x.fkPartyId,
                        principalSchema: "people",
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_CreatedAt",
                schema: "people",
                table: "naturalPersons",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_CreatedBy",
                schema: "people",
                table: "naturalPersons",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_fkPartyId",
                schema: "people",
                table: "naturalPersons",
                column: "fkPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_ModifiedAt",
                schema: "people",
                table: "naturalPersons",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_naturalPersons_ModifiedBy",
                schema: "people",
                table: "naturalPersons",
                column: "ModifiedBy");

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

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_naturalPersons_PersonId",
                schema: "people",
                table: "PersonContacts",
                column: "PersonId",
                principalSchema: "people",
                principalTable: "naturalPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonProfiles_naturalPersons_FkPersonId",
                schema: "people",
                table: "PersonProfiles",
                column: "FkPersonId",
                principalSchema: "people",
                principalTable: "naturalPersons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PersonContacts_naturalPersons_PersonId",
                schema: "people",
                table: "PersonContacts");

            migrationBuilder.DropForeignKey(
                name: "FK_PersonProfiles_naturalPersons_FkPersonId",
                schema: "people",
                table: "PersonProfiles");

            migrationBuilder.DropTable(
                name: "naturalPersons",
                schema: "people");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "PersonProfiles",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "PersonContacts",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "Parties",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedBy",
                schema: "people",
                table: "legalPersons",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "naturalPerson",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    fkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BirthPlace = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NationalCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_PersonContacts_naturalPerson_PersonId",
                schema: "people",
                table: "PersonContacts",
                column: "PersonId",
                principalSchema: "people",
                principalTable: "naturalPerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PersonProfiles_naturalPerson_FkPersonId",
                schema: "people",
                table: "PersonProfiles",
                column: "FkPersonId",
                principalSchema: "people",
                principalTable: "naturalPerson",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
