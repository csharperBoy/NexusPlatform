using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_AddCorporateActions_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CorporateActionsFailedCount",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CorporateActionsFetchedCount",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CorporateActionsLastError",
                schema: "trader",
                table: "HistoricalDataCollectionJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CorporateActions",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ExDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Ratio = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: true),
                    SubscriptionPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RawText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CorporateActions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_Isin_ActionType",
                schema: "trader",
                table: "CorporateActions",
                columns: new[] { "Isin", "ActionType" });

            migrationBuilder.CreateIndex(
                name: "IX_CorporateActions_Isin_ExDateUtc",
                schema: "trader",
                table: "CorporateActions",
                columns: new[] { "Isin", "ExDateUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CorporateActions",
                schema: "trader");

            migrationBuilder.DropColumn(
                name: "CorporateActionsFailedCount",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "CorporateActionsFetchedCount",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");

            migrationBuilder.DropColumn(
                name: "CorporateActionsLastError",
                schema: "trader",
                table: "HistoricalDataCollectionJobs");
        }
    }
}
