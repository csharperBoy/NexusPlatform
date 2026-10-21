using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_3_Trader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceEvaluationId",
                schema: "trader",
                table: "Signals",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceEvaluationId",
                schema: "trader",
                table: "Positions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceEvaluationId",
                schema: "trader",
                table: "OrderIntents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StrategyEvaluations",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Isin = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StrategyKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EvaluatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PriceAtEval = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    FeaturesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    Strength = table.Column<decimal>(type: "decimal(9,4)", precision: 9, scale: 4, nullable: false),
                    SignalId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OutcomeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OutcomeUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrategyEvaluation", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Signal_SourceEvaluationId",
                schema: "trader",
                table: "Signals",
                column: "SourceEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_Position_SourceEvaluationId",
                schema: "trader",
                table: "Positions",
                column: "SourceEvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEval_EvaluatedAt",
                schema: "trader",
                table: "StrategyEvaluations",
                column: "EvaluatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEval_Isin_Time",
                schema: "trader",
                table: "StrategyEvaluations",
                columns: new[] { "Isin", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEval_NonHold",
                schema: "trader",
                table: "StrategyEvaluations",
                column: "Action",
                filter: "[Action] <> 0");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEval_Run_Isin_Time",
                schema: "trader",
                table: "StrategyEvaluations",
                columns: new[] { "RunId", "Isin", "EvaluatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEval_SignalId",
                schema: "trader",
                table: "StrategyEvaluations",
                column: "SignalId");

            migrationBuilder.CreateIndex(
                name: "IX_StrategyEvaluations_Id",
                schema: "trader",
                table: "StrategyEvaluations",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrategyEvaluations",
                schema: "trader");

            migrationBuilder.DropIndex(
                name: "IX_Signal_SourceEvaluationId",
                schema: "trader",
                table: "Signals");

            migrationBuilder.DropIndex(
                name: "IX_Position_SourceEvaluationId",
                schema: "trader",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "SourceEvaluationId",
                schema: "trader",
                table: "Signals");

            migrationBuilder.DropColumn(
                name: "SourceEvaluationId",
                schema: "trader",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "SourceEvaluationId",
                schema: "trader",
                table: "OrderIntents");
        }
    }
}
