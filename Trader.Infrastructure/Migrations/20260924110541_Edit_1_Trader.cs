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
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EncryptedPassword = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    EncryptedToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TokenExp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraderAccount", x => x.Id);
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

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Id",
                schema: "trader",
                table: "Accounts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraderAccount_Username",
                schema: "trader",
                table: "Accounts",
                column: "Username");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "ExecutionLogs",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "ScheduledOrders",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "Symbols",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "SchedulePlans",
                schema: "trader");
        }
    }
}
