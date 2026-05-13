using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Authorization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_3_Authorization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JoinDetails_PermissionRuleId",
                schema: "authorization",
                table: "JoinDetails",
                column: "PermissionRuleId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_JoinDetails_PermissionRules_PermissionRuleId",
                schema: "authorization",
                table: "JoinDetails",
                column: "PermissionRuleId",
                principalSchema: "authorization",
                principalTable: "PermissionRules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JoinDetails_PermissionRules_PermissionRuleId",
                schema: "authorization",
                table: "JoinDetails");

            migrationBuilder.DropIndex(
                name: "IX_JoinDetails_PermissionRuleId",
                schema: "authorization",
                table: "JoinDetails");
        }
    }
}
