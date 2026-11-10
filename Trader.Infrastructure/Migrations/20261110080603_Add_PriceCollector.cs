using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_PriceCollector : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PriceSources",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ConfigJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromAsset = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ToAsset = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceSource", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PriceCollectionJobs",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceSourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Mode = table.Column<int>(type: "int", nullable: false),
                    RecurrencePatternJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    State = table.Column<int>(type: "int", nullable: false),
                    SchedulerRecurringId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SchedulerJobId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StoppedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastRunInserted = table.Column<int>(type: "int", nullable: false),
                    LastRunSkipped = table.Column<int>(type: "int", nullable: false),
                    TotalInserted = table.Column<int>(type: "int", nullable: false),
                    BackfillProgressPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceCollectionJob", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceCollectionJobs_PriceSources_PriceSourceId",
                        column: x => x.PriceSourceId,
                        principalSchema: "trader",
                        principalTable: "PriceSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_CreatedAt",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_CreatedBy",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_Mode",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "Mode");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_ModifiedAt",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_ModifiedBy",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_Source",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "PriceSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJob_State",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_PriceCollectionJobs_Id",
                schema: "trader",
                table: "PriceCollectionJobs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceSource_CreatedAt",
                schema: "trader",
                table: "PriceSources",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSource_CreatedBy",
                schema: "trader",
                table: "PriceSources",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSource_IsActive",
                schema: "trader",
                table: "PriceSources",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSource_ModifiedAt",
                schema: "trader",
                table: "PriceSources",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSource_ModifiedBy",
                schema: "trader",
                table: "PriceSources",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PriceSource_Pair",
                schema: "trader",
                table: "PriceSources",
                columns: new[] { "FromAsset", "ToAsset" });

            migrationBuilder.CreateIndex(
                name: "IX_PriceSources_Id",
                schema: "trader",
                table: "PriceSources",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PriceSource_Code",
                schema: "trader",
                table: "PriceSources",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PriceCollectionJobs",
                schema: "trader");

            migrationBuilder.DropTable(
                name: "PriceSources",
                schema: "trader");
        }
    }
}
