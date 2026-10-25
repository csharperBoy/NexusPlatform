using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_6_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                schema: "trader",
                table: "CurrencyRates",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "trader",
                table: "CurrencyRates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                schema: "trader",
                table: "CurrencyRates",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "trader",
                table: "CurrencyRates",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRate_CreatedAt",
                schema: "trader",
                table: "CurrencyRates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRate_CreatedBy",
                schema: "trader",
                table: "CurrencyRates",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRate_ModifiedAt",
                schema: "trader",
                table: "CurrencyRates",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyRate_ModifiedBy",
                schema: "trader",
                table: "CurrencyRates",
                column: "ModifiedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CurrencyRate_CreatedAt",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyRate_CreatedBy",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyRate_ModifiedAt",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropIndex(
                name: "IX_CurrencyRate_ModifiedBy",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                schema: "trader",
                table: "CurrencyRates");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "trader",
                table: "CurrencyRates");
        }
    }
}
