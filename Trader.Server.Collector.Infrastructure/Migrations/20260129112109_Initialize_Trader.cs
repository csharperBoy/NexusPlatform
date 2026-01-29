using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Server.Collector.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initialize_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "collector");

            migrationBuilder.EnsureSchema(
                name: "trader");

            migrationBuilder.CreateTable(
                name: "OptionContracts",
                schema: "collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    FkStockId = table.Column<Guid>(type: "uniqueidentifier", unicode: false, nullable: false),
                    NumberOfOpenPositionAllow = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquivalentResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionContracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Options",
                schema: "collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkOptionContractId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Side = table.Column<byte>(type: "tinyint", nullable: false),
                    DuePrice = table.Column<long>(type: "bigint", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquivalentResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Options", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "trader",
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
                name: "SnapShotFromOptionTrading",
                schema: "collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPrice = table.Column<int>(type: "int", nullable: true),
                    ClosePrice = table.Column<int>(type: "int", nullable: true),
                    TotalTradesCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalTradedVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalTradedValue = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder1Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder1Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder1Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder2Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder2Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder2Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder3Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder3Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder3Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder4Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder4Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder4Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder5Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder5Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder5Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder1Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder1Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder1Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder2Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder2Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder2Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder3Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder3Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder3Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder4Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder4Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder4Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder5Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder5Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder5Count = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyOrderVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellOrderVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyOrdersCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellOrdersCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyTruePersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyTruePersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellTruePersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellTruePersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyLegalPersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyLegalPersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellLegalPersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellLegalPersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    NumberOfOpenPositions = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapShotFromOptionTrading", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SnapShotFromStockTrading",
                schema: "collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkStockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastPrice = table.Column<int>(type: "int", nullable: true),
                    ClosePrice = table.Column<int>(type: "int", nullable: true),
                    TotalTradesCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalTradedVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalTradedValue = table.Column<long>(type: "bigint", nullable: true),
                    MarketCapitalization = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder1Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder1Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder1Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder2Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder2Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder2Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder3Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder3Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder3Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder4Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder4Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder4Count = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder5Price = table.Column<int>(type: "int", nullable: true),
                    BuyOrder5Volume = table.Column<long>(type: "bigint", nullable: true),
                    BuyOrder5Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder1Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder1Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder1Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder2Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder2Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder2Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder3Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder3Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder3Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder4Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder4Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder4Count = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder5Price = table.Column<int>(type: "int", nullable: true),
                    SellOrder5Volume = table.Column<long>(type: "bigint", nullable: true),
                    SellOrder5Count = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyOrderVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellOrderVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyOrdersCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellOrdersCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyTruePersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyTruePersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellTruePersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellTruePersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyLegalPersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalBuyLegalPersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellLegalPersonalityVolume = table.Column<long>(type: "bigint", nullable: true),
                    TotalSellLegalPersonalityCount = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapShotFromStockTrading", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                schema: "collector",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TypeOfMarket = table.Column<byte>(type: "tinyint", nullable: true),
                    PreOpeningTimeStart = table.Column<TimeOnly>(type: "time", nullable: true),
                    PreOpeningTimeEnd = table.Column<TimeOnly>(type: "time", nullable: true),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    TPlus = table.Column<int>(type: "int", nullable: true),
                    BuyCommissionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    SellCommissionRate = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinValueBuyOrder = table.Column<int>(type: "int", nullable: true),
                    MinValueSellOrder = table.Column<int>(type: "int", nullable: true),
                    StepPrice = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    CodeOfTsetmc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PercentOfDailyTolerance = table.Column<double>(type: "float", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EquivalentResourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_CreatedAt",
                schema: "collector",
                table: "OptionContracts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_CreatedBy",
                schema: "collector",
                table: "OptionContracts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_FastLookup",
                schema: "collector",
                table: "OptionContracts",
                columns: new[] { "FkStockId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_ModifiedAt",
                schema: "collector",
                table: "OptionContracts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_ModifiedBy",
                schema: "collector",
                table: "OptionContracts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_OwnerOrgUnit",
                schema: "collector",
                table: "OptionContracts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_OwnerPerson",
                schema: "collector",
                table: "OptionContracts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_OptionContract_ScopedLookup",
                schema: "collector",
                table: "OptionContracts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Option_CreatedAt",
                schema: "collector",
                table: "Options",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Option_CreatedBy",
                schema: "collector",
                table: "Options",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Option_FastLookup",
                schema: "collector",
                table: "Options",
                columns: new[] { "Side", "FkOptionContractId", "DuePrice" });

            migrationBuilder.CreateIndex(
                name: "IX_Option_ModifiedAt",
                schema: "collector",
                table: "Options",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Option_ModifiedBy",
                schema: "collector",
                table: "Options",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Option_OwnerOrgUnit",
                schema: "collector",
                table: "Options",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Option_OwnerPerson",
                schema: "collector",
                table: "Options",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Option_ScopedLookup",
                schema: "collector",
                table: "Options",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Option_Title_FastLookup",
                schema: "collector",
                table: "Options",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "trader",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "trader",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "trader",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_CreatedAt",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_CreatedBy",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_FastLookup",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                columns: new[] { "DateTime", "FkOptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_ModifiedAt",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_ModifiedBy",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_OwnerOrgUnit",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_OwnerPerson",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromOptionTrading_ScopedLookup",
                schema: "collector",
                table: "SnapShotFromOptionTrading",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_CreatedAt",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_CreatedBy",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_FastLookup",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                columns: new[] { "DateTime", "FkStockId" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_ModifiedAt",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_ModifiedBy",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_OwnerOrgUnit",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_OwnerPerson",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapShotFromStockTrading_ScopedLookup",
                schema: "collector",
                table: "SnapShotFromStockTrading",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Stock_CreatedAt",
                schema: "collector",
                table: "Stock",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_CreatedBy",
                schema: "collector",
                table: "Stock",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_FastLookup",
                schema: "collector",
                table: "Stock",
                columns: new[] { "Isin", "Title", "OpenTime", "TypeOfMarket" });

            migrationBuilder.CreateIndex(
                name: "IX_Stock_Isin_FastLookup",
                schema: "collector",
                table: "Stock",
                column: "Isin");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ModifiedAt",
                schema: "collector",
                table: "Stock",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ModifiedBy",
                schema: "collector",
                table: "Stock",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_OwnerOrgUnit",
                schema: "collector",
                table: "Stock",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_OwnerPerson",
                schema: "collector",
                table: "Stock",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_ScopedLookup",
                schema: "collector",
                table: "Stock",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionContracts",
                schema: "collector");

            migrationBuilder.DropTable(
                name: "Options",
                schema: "collector");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "SnapShotFromOptionTrading",
                schema: "collector");

            migrationBuilder.DropTable(
                name: "SnapShotFromStockTrading",
                schema: "collector");

            migrationBuilder.DropTable(
                name: "Stock",
                schema: "collector");
        }
    }
}
