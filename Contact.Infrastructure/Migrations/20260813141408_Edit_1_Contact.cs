using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Edit_1_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignment",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "EmploymentLocations",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "PostLocations",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "Employment",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "Location",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "Post",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "EmploymentStatus",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "EmploymentType",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "CostCenter",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "Grade",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "JobLevel",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "JobTitle",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "OrganizationUnit",
                schema: "contact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CostCenter",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentStatus",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentType",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grade",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobLevel",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobTitle",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobTitle", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Location",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Location", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Location_Location_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "contact",
                        principalTable: "Location",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnit",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationUnit_OrganizationUnit_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "contact",
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Employment",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    EmploymentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkEmploymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkEmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkNaturalPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employment_EmploymentStatus_EmploymentStatusId",
                        column: x => x.EmploymentStatusId,
                        principalSchema: "contact",
                        principalTable: "EmploymentStatus",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Employment_EmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalSchema: "contact",
                        principalTable: "EmploymentType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Post",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkCostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkJobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkJobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkPermissionAssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Post", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Post_CostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "contact",
                        principalTable: "CostCenter",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_Grade_GradeId",
                        column: x => x.GradeId,
                        principalSchema: "contact",
                        principalTable: "Grade",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_JobLevel_JobLevelId",
                        column: x => x.JobLevelId,
                        principalSchema: "contact",
                        principalTable: "JobLevel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_JobTitle_JobTitleId",
                        column: x => x.JobTitleId,
                        principalSchema: "contact",
                        principalTable: "JobTitle",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Post_OrganizationUnit_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalSchema: "contact",
                        principalTable: "OrganizationUnit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Post_Post_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "contact",
                        principalTable: "Post",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmploymentLocations",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmploymentLocations_Employment_EmploymentId",
                        column: x => x.EmploymentId,
                        principalSchema: "contact",
                        principalTable: "Employment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmploymentLocations_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "contact",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Assignment",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Assignment_Employment_EmploymentId",
                        column: x => x.EmploymentId,
                        principalSchema: "contact",
                        principalTable: "Employment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Assignment_Post_PostId",
                        column: x => x.PostId,
                        principalSchema: "contact",
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PostLocations",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PostLocations_Location_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "contact",
                        principalTable: "Location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PostLocations_Post_PostId",
                        column: x => x.PostId,
                        principalSchema: "contact",
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_EmploymentId",
                schema: "contact",
                table: "Assignment",
                column: "EmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_PostId",
                schema: "contact",
                table: "Assignment",
                column: "PostId");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_EmploymentStatusId",
                schema: "contact",
                table: "Employment",
                column: "EmploymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Employment_EmploymentTypeId",
                schema: "contact",
                table: "Employment",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_EmploymentId",
                schema: "contact",
                table: "EmploymentLocations",
                column: "EmploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentLocations_LocationId",
                schema: "contact",
                table: "EmploymentLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Location_ParentId",
                schema: "contact",
                table: "Location",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ParentId",
                schema: "contact",
                table: "OrganizationUnit",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_CostCenterId",
                schema: "contact",
                table: "Post",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_GradeId",
                schema: "contact",
                table: "Post",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_JobLevelId",
                schema: "contact",
                table: "Post",
                column: "JobLevelId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_JobTitleId",
                schema: "contact",
                table: "Post",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_OrganizationUnitId",
                schema: "contact",
                table: "Post",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Post_ParentId",
                schema: "contact",
                table: "Post",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_LocationId",
                schema: "contact",
                table: "PostLocations",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PostLocations_PostId",
                schema: "contact",
                table: "PostLocations",
                column: "PostId");
        }
    }
}
