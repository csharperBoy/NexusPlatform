using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HR.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_HR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.CreateTable(
                name: " CostCenter",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: " EmploymentStatus",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: " EmploymentType",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: " Grade",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobLevel",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTitle",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_Location_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "hr",
                        principalTable: "Location",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnits",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Path = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnits_OrganizationUnits_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "hr",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "hr",
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
                name: " Employment",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkNaturalPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ Employment_ EmploymentStatus1",
                        column: x => x.FkEmploymentStatusId,
                        principalSchema: "hr",
                        principalTable: " EmploymentStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ Employment_ EmploymentType1",
                        column: x => x.FkEmploymentTypeId,
                        principalSchema: "hr",
                        principalTable: " EmploymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Post",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkJobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkJobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkCostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FkPermissionAssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Post_ CostCenter",
                        column: x => x.FkCostCenterId,
                        principalSchema: "hr",
                        principalTable: " CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_ Grade",
                        column: x => x.FkGradeId,
                        principalSchema: "hr",
                        principalTable: " Grade",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_JobLevel",
                        column: x => x.FkJobLevelId,
                        principalSchema: "hr",
                        principalTable: "JobLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_JobTitle",
                        column: x => x.FkJobTitleId,
                        principalSchema: "hr",
                        principalTable: "JobTitle",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_OrganizationUnits",
                        column: x => x.FkOrganizationUnitId,
                        principalSchema: "hr",
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_Post_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "hr",
                        principalTable: "Post",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: " EmploymentLocations",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentLocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ EmploymentLocations_ Employment",
                        column: x => x.FkEmployeeId,
                        principalSchema: "hr",
                        principalTable: " Employment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ EmploymentLocations_Location",
                        column: x => x.FkLocationId,
                        principalSchema: "hr",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assignments",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignments_ Employment",
                        column: x => x.FkEmploymentId,
                        principalSchema: "hr",
                        principalTable: " Employment",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Assignments_Post",
                        column: x => x.FkPostId,
                        principalSchema: "hr",
                        principalTable: "Post",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ CostCenter_Id",
                schema: "hr",
                table: " CostCenter",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ Employment_FkEmploymentStatusId",
                schema: "hr",
                table: " Employment",
                column: "FkEmploymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ Employment_FkEmploymentTypeId",
                schema: "hr",
                table: " Employment",
                column: "FkEmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ Employment_Id",
                schema: "hr",
                table: " Employment",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_fkEmployeeId",
                schema: "hr",
                table: " EmploymentLocations",
                column: "FkEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_fkLocationId",
                schema: "hr",
                table: " EmploymentLocations",
                column: "FkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentLocations_Id",
                schema: "hr",
                table: " EmploymentLocations",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_CreatedAt",
                schema: "hr",
                table: " EmploymentLocations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_CreatedBy",
                schema: "hr",
                table: " EmploymentLocations",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_ModifiedAt",
                schema: "hr",
                table: " EmploymentLocations",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocation_ModifiedBy",
                schema: "hr",
                table: " EmploymentLocations",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentStatus_Id",
                schema: "hr",
                table: " EmploymentStatus",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ EmploymentType_Id",
                schema: "hr",
                table: " EmploymentType",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ Grade_Id",
                schema: "hr",
                table: " Grade",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_CreatedAt",
                schema: "hr",
                table: "Assignments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_CreatedBy",
                schema: "hr",
                table: "Assignments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ModifiedAt",
                schema: "hr",
                table: "Assignments",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_ModifiedBy",
                schema: "hr",
                table: "Assignments",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_EmploymentId",
                schema: "hr",
                table: "Assignments",
                column: "FkEmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_Id",
                schema: "hr",
                table: "Assignments",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Assignments_PostId",
                schema: "hr",
                table: "Assignments",
                column: "FkPostId");

            migrationBuilder.CreateIndex(
                name: "IX_JobLevel_Id",
                schema: "hr",
                table: "JobLevel",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobTitle_Id",
                schema: "hr",
                table: "JobTitle",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_CreatedAt",
                schema: "hr",
                table: "Location",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Location_CreatedBy",
                schema: "hr",
                table: "Location",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Location_Id",
                schema: "hr",
                table: "Location",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Location_ModifiedAt",
                schema: "hr",
                table: "Location",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ModifiedBy",
                schema: "hr",
                table: "Location",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ParentId",
                schema: "hr",
                table: "Location",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_CreatedAt",
                schema: "hr",
                table: "OrganizationUnits",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_CreatedBy",
                schema: "hr",
                table: "OrganizationUnits",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ModifiedAt",
                schema: "hr",
                table: "OrganizationUnits",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ModifiedBy",
                schema: "hr",
                table: "OrganizationUnits",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Code",
                schema: "hr",
                table: "OrganizationUnits",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Id",
                schema: "hr",
                table: "OrganizationUnits",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_ParentId",
                schema: "hr",
                table: "OrganizationUnits",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnits_Path",
                schema: "hr",
                table: "OrganizationUnits",
                column: "Path");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "hr",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "hr",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "hr",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CostCenterId",
                schema: "hr",
                table: "Post",
                column: "FkCostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CreatedAt",
                schema: "hr",
                table: "Post",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CreatedBy",
                schema: "hr",
                table: "Post",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Post_GradeId",
                schema: "hr",
                table: "Post",
                column: "FkGradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_Id",
                schema: "hr",
                table: "Post",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Post_JobLevelId",
                schema: "hr",
                table: "Post",
                column: "FkJobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_JobTitleId",
                schema: "hr",
                table: "Post",
                column: "FkJobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ModifiedAt",
                schema: "hr",
                table: "Post",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ModifiedBy",
                schema: "hr",
                table: "Post",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Post_OrganizationUnitId",
                schema: "hr",
                table: "Post",
                column: "FkOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ParentId",
                schema: "hr",
                table: "Post",
                column: "ParentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: " EmploymentLocations",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "Assignments",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " Employment",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "Post",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " EmploymentStatus",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " EmploymentType",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " CostCenter",
                schema: "hr");

            migrationBuilder.DropTable(
                name: " Grade",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "JobLevel",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "JobTitle",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "OrganizationUnits",
                schema: "hr");
        }
    }
}
