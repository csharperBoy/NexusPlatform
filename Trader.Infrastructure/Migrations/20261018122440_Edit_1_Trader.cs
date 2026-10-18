using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "trader");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Broker = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EncryptedSession = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SessionExp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderAccount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BrokerOrders",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIntentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrokerOrderId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    FilledQuantity = table.Column<long>(type: "bigint", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BrokerOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExecutionLogs",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Timestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Code = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExecutionLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Instruments",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Market = table.Column<int>(type: "int", nullable: false),
                    BoardCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OptionType = table.Column<int>(type: "int", nullable: true),
                    OptionStrikePrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    OptionExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    OptionUnderlyingIsin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Instrument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderIntents",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSignalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    OrderMode = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DispatchedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderIntent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderTrades",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BrokerOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeNumber = table.Column<long>(type: "bigint", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    TradeDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsCanceled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CancelDateTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderTrade", x => x.Id);
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
                name: "Positions",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    AverageEntryPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RealizedPnLAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    RealizedPnLCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    UnrealizedPnLAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    UnrealizedPnLCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StopLossPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TakeProfitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    OpenedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Position", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchedulePlans",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    AutoLoginAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    AutoRefreshAt = table.Column<TimeOnly>(type: "time", nullable: false),
                    FireLeadMs = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulePlan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Signals",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    StrategyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProducedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConsumedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Symbols",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SymbolName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SymbolIsin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    ValidityType = table.Column<int>(type: "int", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,8)", precision: 18, scale: 8, nullable: false),
                    OrderModelType = table.Column<int>(type: "int", nullable: false),
                    OrderFrom = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderSymbol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradingRuns",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    State = table.Column<int>(type: "int", nullable: false),
                    StrategyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StrategyConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PrimaryTimeFrame = table.Column<int>(type: "int", nullable: false),
                    CapitalAllocationMode = table.Column<int>(type: "int", nullable: false),
                    FixedCapitalAmount = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    FixedCapitalCurrency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    StopLossPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    StopLossPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    TakeProfitPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: true),
                    TakeProfitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DefaultOrderMode = table.Column<int>(type: "int", nullable: false),
                    OnMarketClose = table.Column<int>(type: "int", nullable: false),
                    OnStop = table.Column<int>(type: "int", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoppedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingRun", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstrumentOrderPresets",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultQuantity = table.Column<long>(type: "bigint", nullable: false),
                    DefaultSide = table.Column<int>(type: "int", nullable: false),
                    ValidityType = table.Column<int>(type: "int", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefaultOrderMode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstrumentOrderPreset", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InstrumentOrderPresets_Instruments_InstrumentId",
                        column: x => x.InstrumentId,
                        principalSchema: "trader",
                        principalTable: "Instruments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledOrders",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SymbolIsin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<long>(type: "bigint", nullable: false),
                    TotalValue = table.Column<long>(type: "bigint", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    Fired = table.Column<bool>(type: "bit", nullable: false),
                    FiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchedulePlan_Orders",
                        column: x => x.PlanId,
                        principalSchema: "trader",
                        principalTable: "SchedulePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradingRunAccounts",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradingRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraderAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingRunAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingRunAccounts_TradingRuns_TradingRunId",
                        column: x => x.TradingRunId,
                        principalSchema: "trader",
                        principalTable: "TradingRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradingRunSymbols",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradingRunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AllocationPercentage = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradingRunSymbol", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradingRunSymbols_TradingRuns_TradingRunId",
                        column: x => x.TradingRunId,
                        principalSchema: "trader",
                        principalTable: "TradingRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Id",
                schema: "trader",
                table: "Accounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraderAccount_Broker",
                schema: "trader",
                table: "Accounts",
                column: "Broker");

            migrationBuilder.CreateIndex(
                name: "IX_TraderAccount_Username",
                schema: "trader",
                table: "Accounts",
                column: "Username");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_Account_ExternalId",
                schema: "trader",
                table: "BrokerOrders",
                columns: new[] { "TraderAccountId", "BrokerOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_Account_State",
                schema: "trader",
                table: "BrokerOrders",
                columns: new[] { "TraderAccountId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_CreatedAt",
                schema: "trader",
                table: "BrokerOrders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_CreatedBy",
                schema: "trader",
                table: "BrokerOrders",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_ModifiedAt",
                schema: "trader",
                table: "BrokerOrders",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_ModifiedBy",
                schema: "trader",
                table: "BrokerOrders",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrder_Run_State",
                schema: "trader",
                table: "BrokerOrders",
                columns: new[] { "RunId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerOrders_Id",
                schema: "trader",
                table: "BrokerOrders",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLog_PlanId",
                schema: "trader",
                table: "ExecutionLogs",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLog_Timestamp",
                schema: "trader",
                table: "ExecutionLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_ExecutionLogs_Id",
                schema: "trader",
                table: "ExecutionLogs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentOrderPreset_CreatedAt",
                schema: "trader",
                table: "InstrumentOrderPresets",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentOrderPreset_CreatedBy",
                schema: "trader",
                table: "InstrumentOrderPresets",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentOrderPreset_InstrumentId",
                schema: "trader",
                table: "InstrumentOrderPresets",
                column: "InstrumentId");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentOrderPreset_ModifiedAt",
                schema: "trader",
                table: "InstrumentOrderPresets",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentOrderPreset_ModifiedBy",
                schema: "trader",
                table: "InstrumentOrderPresets",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_InstrumentOrderPresets_Id",
                schema: "trader",
                table: "InstrumentOrderPresets",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_CreatedAt",
                schema: "trader",
                table: "Instruments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_CreatedBy",
                schema: "trader",
                table: "Instruments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_IsActive",
                schema: "trader",
                table: "Instruments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_Market",
                schema: "trader",
                table: "Instruments",
                column: "Market");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_ModifiedAt",
                schema: "trader",
                table: "Instruments",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Instrument_ModifiedBy",
                schema: "trader",
                table: "Instruments",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Instruments_Id",
                schema: "trader",
                table: "Instruments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Instrument_Isin",
                schema: "trader",
                table: "Instruments",
                column: "Isin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_Account_Isin",
                schema: "trader",
                table: "OrderIntents",
                columns: new[] { "TraderAccountId", "Isin" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_CreatedAt",
                schema: "trader",
                table: "OrderIntents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_CreatedBy",
                schema: "trader",
                table: "OrderIntents",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_ModifiedAt",
                schema: "trader",
                table: "OrderIntents",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_ModifiedBy",
                schema: "trader",
                table: "OrderIntents",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntent_Run_State",
                schema: "trader",
                table: "OrderIntents",
                columns: new[] { "RunId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_OrderIntents_Id",
                schema: "trader",
                table: "OrderIntents",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrade_TradeNumber",
                schema: "trader",
                table: "OrderTrades",
                column: "TradeNumber");

            migrationBuilder.CreateIndex(
                name: "IX_OrderTrades_Id",
                schema: "trader",
                table: "OrderTrades",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_OrderTrade_BrokerOrder_TradeNumber",
                schema: "trader",
                table: "OrderTrades",
                columns: new[] { "BrokerOrderId", "TradeNumber" },
                unique: true);

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
                name: "IX_Position_Account_Isin",
                schema: "trader",
                table: "Positions",
                columns: new[] { "TraderAccountId", "Isin" });

            migrationBuilder.CreateIndex(
                name: "IX_Position_CreatedAt",
                schema: "trader",
                table: "Positions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Position_CreatedBy",
                schema: "trader",
                table: "Positions",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Position_ModifiedAt",
                schema: "trader",
                table: "Positions",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Position_ModifiedBy",
                schema: "trader",
                table: "Positions",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Position_Run_Isin_State",
                schema: "trader",
                table: "Positions",
                columns: new[] { "RunId", "Isin", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_Positions_Id",
                schema: "trader",
                table: "Positions",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledOrder_AccountId",
                schema: "trader",
                table: "ScheduledOrders",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledOrder_PlanId",
                schema: "trader",
                table: "ScheduledOrders",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledOrders_Id",
                schema: "trader",
                table: "ScheduledOrders",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlan_Date",
                schema: "trader",
                table: "SchedulePlans",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlan_Date_Enabled",
                schema: "trader",
                table: "SchedulePlans",
                columns: new[] { "Date", "Enabled" });

            migrationBuilder.CreateIndex(
                name: "IX_SchedulePlans_Id",
                schema: "trader",
                table: "SchedulePlans",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Signal_Run_Isin",
                schema: "trader",
                table: "Signals",
                columns: new[] { "RunId", "Isin" });

            migrationBuilder.CreateIndex(
                name: "IX_Signal_Run_ProducedAt",
                schema: "trader",
                table: "Signals",
                columns: new[] { "RunId", "ProducedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Signals_Id",
                schema: "trader",
                table: "Signals",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Symbols_Id",
                schema: "trader",
                table: "Symbols",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraderSymbol_SymbolIsin",
                schema: "trader",
                table: "Symbols",
                column: "SymbolIsin",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradingRunAccount_Account",
                schema: "trader",
                table: "TradingRunAccounts",
                column: "TraderAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRunAccounts_Id",
                schema: "trader",
                table: "TradingRunAccounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TradingRunAccount_Run_Account",
                schema: "trader",
                table: "TradingRunAccounts",
                columns: new[] { "TradingRunId", "TraderAccountId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_CreatedAt",
                schema: "trader",
                table: "TradingRuns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_CreatedBy",
                schema: "trader",
                table: "TradingRuns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_ModifiedAt",
                schema: "trader",
                table: "TradingRuns",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_ModifiedBy",
                schema: "trader",
                table: "TradingRuns",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_Owner_State",
                schema: "trader",
                table: "TradingRuns",
                columns: new[] { "OwnerPersonId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_OwnerOrgUnit",
                schema: "trader",
                table: "TradingRuns",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_OwnerPerson",
                schema: "trader",
                table: "TradingRuns",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_ScopedLookup",
                schema: "trader",
                table: "TradingRuns",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_State",
                schema: "trader",
                table: "TradingRuns",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRun_StrategyKey",
                schema: "trader",
                table: "TradingRuns",
                column: "StrategyKey");

            migrationBuilder.CreateIndex(
                name: "IX_TradingRuns_Id",
                schema: "trader",
                table: "TradingRuns",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TradingRunSymbols_Id",
                schema: "trader",
                table: "TradingRunSymbols",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_TradingRunSymbol_Run_Isin",
                schema: "trader",
                table: "TradingRunSymbols",
                columns: new[] { "TradingRunId", "Isin" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "BrokerOrders",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "ExecutionLogs",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "InstrumentOrderPresets",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "OrderIntents",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "OrderTrades",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "Positions",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "ScheduledOrders",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "Signals",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "Symbols",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "TradingRunAccounts",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "TradingRunSymbols",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "Instruments",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "SchedulePlans",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "TradingRuns",
                schema: "trader");
        }
    }
}
