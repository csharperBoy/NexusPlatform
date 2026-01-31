using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Trader.Server.Collector.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_Trader1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Stock",
                schema: "collector",
                newName: "Stock",
                newSchema: "trader");

            migrationBuilder.RenameTable(
                name: "SnapShotFromStockTrading",
                schema: "collector",
                newName: "SnapShotFromStockTrading",
                newSchema: "trader");

            migrationBuilder.RenameTable(
                name: "SnapShotFromOptionTrading",
                schema: "collector",
                newName: "SnapShotFromOptionTrading",
                newSchema: "trader");

            migrationBuilder.RenameTable(
                name: "Options",
                schema: "collector",
                newName: "Options",
                newSchema: "trader");

            migrationBuilder.RenameTable(
                name: "OptionContracts",
                schema: "collector",
                newName: "OptionContracts",
                newSchema: "trader");

            migrationBuilder.CreateTable(
                name: "BrokerageAccount",
                schema: "trader",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Platform = table.Column<byte>(type: "tinyint", nullable: false),
                    FkUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_BrokerageAccount", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_CreatedAt",
                schema: "trader",
                table: "BrokerageAccount",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_CreatedBy",
                schema: "trader",
                table: "BrokerageAccount",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_ModifiedAt",
                schema: "trader",
                table: "BrokerageAccount",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_ModifiedBy",
                schema: "trader",
                table: "BrokerageAccount",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_OwnerOrgUnit",
                schema: "trader",
                table: "BrokerageAccount",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_OwnerPerson",
                schema: "trader",
                table: "BrokerageAccount",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_BrokerageAccount_ScopedLookup",
                schema: "trader",
                table: "BrokerageAccount",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_Option_FastLookup",
                schema: "trader",
                table: "BrokerageAccount",
                columns: new[] { "Platform", "FkUserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BrokerageAccount",
                schema: "trader");

            migrationBuilder.EnsureSchema(
                name: "collector");

            migrationBuilder.RenameTable(
                name: "Stock",
                schema: "trader",
                newName: "Stock",
                newSchema: "collector");

            migrationBuilder.RenameTable(
                name: "SnapShotFromStockTrading",
                schema: "trader",
                newName: "SnapShotFromStockTrading",
                newSchema: "collector");

            migrationBuilder.RenameTable(
                name: "SnapShotFromOptionTrading",
                schema: "trader",
                newName: "SnapShotFromOptionTrading",
                newSchema: "collector");

            migrationBuilder.RenameTable(
                name: "Options",
                schema: "trader",
                newName: "Options",
                newSchema: "collector");

            migrationBuilder.RenameTable(
                name: "OptionContracts",
                schema: "trader",
                newName: "OptionContracts",
                newSchema: "collector");
        }
    }
}
