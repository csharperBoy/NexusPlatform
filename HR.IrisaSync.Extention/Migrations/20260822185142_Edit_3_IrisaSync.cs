using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.IrisaSync.Extention.Migrations
{
    /// <inheritdoc />
    public partial class Edit_3_IrisaSync : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "IrisaParentId",
                schema: "hr",
                table: "IrisaSyncOrganizationUnitMap",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IrisaParentId",
                schema: "hr",
                table: "IrisaSyncOrganizationUnitMap");
        }
    }
}
