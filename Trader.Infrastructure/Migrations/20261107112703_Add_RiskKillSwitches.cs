using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_RiskKillSwitches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiskKillSwitches",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ActivatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeactivatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiskKillSwitch", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitch_Account_Active",
                schema: "trader",
                table: "RiskKillSwitches",
                columns: new[] { "AccountId", "DeactivatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitch_Active",
                schema: "trader",
                table: "RiskKillSwitches",
                column: "DeactivatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitch_CreatedAt",
                schema: "trader",
                table: "RiskKillSwitches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitch_CreatedBy",
                schema: "trader",
                table: "RiskKillSwitches",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitch_ModifiedAt",
                schema: "trader",
                table: "RiskKillSwitches",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitch_ModifiedBy",
                schema: "trader",
                table: "RiskKillSwitches",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_RiskKillSwitches_Id",
                schema: "trader",
                table: "RiskKillSwitches",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiskKillSwitches",
                schema: "trader");
        }
    }
}
