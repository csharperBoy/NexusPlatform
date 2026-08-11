using Core.Domain.Common;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using HR.Domain.Entities;
using HR.Infrastructure.Configurations;
using HR.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace HR.Infrastructure.Data
{


    public class HRDbContext : Base_DbContext
    {
        public HRDbContext(
            DbContextOptions<HRDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }
        public HRDbContext(DbContextOptions<HRDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) 
        {
        }
        public virtual DbSet<Assignment> Assignments { get; set; }
        public virtual DbSet<CostCenter> CostCenters { get; set; }
        public virtual DbSet<Employment> Employments { get; set; }
        public virtual DbSet<EmploymentContact> EmploymentContacts { get; set; }
        public virtual DbSet<EmploymentStatus> EmploymentStatuses { get; set; }
        public virtual DbSet<EmploymentType> EmploymentTypes { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<JobLevel> JobLevels { get; set; }
        public virtual DbSet<JobTitle> JobTitles { get; set; }
        public virtual DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<PostContact> PostContacts { get; set; }
        public virtual DbSet<PostLocation> PostLocations { get; set; }
        public virtual DbSet<LocationContact> LocationContacts { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<EmploymentLocation> EmploymentLocations { get; set; }

        public virtual DbSet<EmploymentInfoView> EmployementInfoViews { get; set; }
        public virtual DbSet<PostInfoView> PostInfoViews { get; set; }
        public override void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureTrigger("HR.Infrastructure.SqlScript", "CreateAssignmentTrigger.sql", "trg_Assignments_CheckOverlap");
        }
        public override void EnsureViews(CancellationToken cancellationToken = default)
        {
            EnsureView("HR.Infrastructure.SqlScript", "CreateEmploymentInfoViewScript.sql", "Employment_Info_View","hr");
            EnsureView("HR.Infrastructure.SqlScript", "CreatePostInfoViewScript.sql", "Post_Info_View", "hr");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.HasDefaultSchema("hr");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("hr"));
            modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new CostCenterConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentContactConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentStatusConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GradeConfiguration());
            modelBuilder.ApplyConfiguration(new JobLevelConfiguration());
            modelBuilder.ApplyConfiguration(new JobTitleConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new PostContactConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentLocationsConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentInfoViewConfiguration());
            modelBuilder.ApplyConfiguration(new PostInfoViewConfiguration());
            modelBuilder.ApplyConfiguration(new LocationContactConfiguration());
            modelBuilder.ApplyConfiguration(new PostLocationsConfiguration());

        }
    }

}
