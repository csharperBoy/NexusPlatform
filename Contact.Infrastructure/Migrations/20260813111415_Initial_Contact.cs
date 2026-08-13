using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contact.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Contact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "contact");

            migrationBuilder.EnsureSchema(
                name: "hr");

            migrationBuilder.EnsureSchema(
                name: "people");

            migrationBuilder.CreateTable(
                name: "CostCenter",
                schema: "contact",
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
                name: "EmploymentContacts",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentStatus",
                schema: "contact",
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
                name: "EmploymentType",
                schema: "contact",
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
                name: "Grade",
                schema: "contact",
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
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                        principalSchema: "contact",
                        principalTable: "Location",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LocationContacts",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationUnit",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                name: "OutboxMessages",
                schema: "contact",
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
                name: "PartyContacts",
                schema: "people",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkPartyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PostContacts",
                schema: "hr",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    OwnerOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    ContactType = table.Column<byte>(type: "tinyint", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PostContact", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employment",
                schema: "contact",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmploymentCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkNaturalPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkEmploymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    EmploymentStatusId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FkParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkJobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkOrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkJobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkGradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FkCostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    FkPermissionAssigneeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobLevelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    JobTitleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkEmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssigneeType = table.Column<int>(type: "int", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmploymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    FkLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FkPostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PostId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
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
                name: "IX_EmploymentContact_CreatedAt",
                schema: "hr",
                table: "EmploymentContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_CreatedBy",
                schema: "hr",
                table: "EmploymentContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ModifiedAt",
                schema: "hr",
                table: "EmploymentContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ModifiedBy",
                schema: "hr",
                table: "EmploymentContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_OwnerOrgUnit",
                schema: "hr",
                table: "EmploymentContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_OwnerPerson",
                schema: "hr",
                table: "EmploymentContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContact_ScopedLookup",
                schema: "hr",
                table: "EmploymentContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentContacts_Id",
                schema: "hr",
                table: "EmploymentContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkEmploymentId",
                schema: "hr",
                table: "EmploymentContacts",
                column: "FkEmploymentId");

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
                name: "IX_LocationContact_CreatedAt",
                schema: "hr",
                table: "LocationContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_CreatedBy",
                schema: "hr",
                table: "LocationContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ModifiedAt",
                schema: "hr",
                table: "LocationContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ModifiedBy",
                schema: "hr",
                table: "LocationContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_OwnerOrgUnit",
                schema: "hr",
                table: "LocationContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_OwnerPerson",
                schema: "hr",
                table: "LocationContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContact_ScopedLookup",
                schema: "hr",
                table: "LocationContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationContacts_FkLocationId",
                schema: "hr",
                table: "LocationContacts",
                column: "FkLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationContacts_Id",
                schema: "hr",
                table: "LocationContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationUnit_ParentId",
                schema: "contact",
                table: "OrganizationUnit",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                schema: "contact",
                table: "OutboxMessages",
                column: "ProcessedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_OccurredOnUtc",
                schema: "contact",
                table: "OutboxMessages",
                columns: new[] { "Status", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_TypeName",
                schema: "contact",
                table: "OutboxMessages",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_CreatedAt",
                schema: "people",
                table: "PartyContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_CreatedBy",
                schema: "people",
                table: "PartyContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ModifiedAt",
                schema: "people",
                table: "PartyContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ModifiedBy",
                schema: "people",
                table: "PartyContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_OwnerOrgUnit",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_OwnerPerson",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyContact_ScopedLookup",
                schema: "people",
                table: "PartyContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PartyContacts_Id",
                schema: "people",
                table: "PartyContacts",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerOrgUnit",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_OwnerPerson",
                schema: "people",
                table: "PartyContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PersonContact_ScopedLookup",
                schema: "people",
                table: "PartyContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PersonContacts_FkPartyId",
                schema: "people",
                table: "PartyContacts",
                column: "FkPartyId");

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
                name: "IX_PersonContacts_FkPostId",
                schema: "hr",
                table: "PostContacts",
                column: "FkPostId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_CreatedAt",
                schema: "hr",
                table: "PostContacts",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_CreatedBy",
                schema: "hr",
                table: "PostContacts",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ModifiedAt",
                schema: "hr",
                table: "PostContacts",
                column: "ModifiedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ModifiedBy",
                schema: "hr",
                table: "PostContacts",
                column: "ModifiedBy");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_OwnerOrgUnit",
                schema: "hr",
                table: "PostContacts",
                column: "OwnerOrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_OwnerPerson",
                schema: "hr",
                table: "PostContacts",
                column: "OwnerPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_PostContact_ScopedLookup",
                schema: "hr",
                table: "PostContacts",
                columns: new[] { "OwnerOrganizationUnitId", "OwnerPersonId" });

            migrationBuilder.CreateIndex(
                name: "IX_PostContacts_Id",
                schema: "hr",
                table: "PostContacts",
                column: "Id",
                unique: true);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assignment",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "EmploymentContacts",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "EmploymentLocations",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "LocationContacts",
                schema: "hr");

            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "contact");

            migrationBuilder.DropTable(
                name: "PartyContacts",
                schema: "people");

            migrationBuilder.DropTable(
                name: "PostContacts",
                schema: "hr");

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
    }
}
