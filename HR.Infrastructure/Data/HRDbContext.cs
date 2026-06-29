using Core.Domain.Common;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using HR.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HR.Domain.Entities;
using HR.Infrastructure.Services;
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
        public virtual DbSet<EmploymentStatus> EmploymentStatuses { get; set; }
        public virtual DbSet<EmploymentType> EmploymentTypes { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
        public virtual DbSet<JobLevel> JobLevels { get; set; }
        public virtual DbSet<JobTitle> JobTitles { get; set; }
        public virtual DbSet<OrganizationUnit> OrganizationUnits { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Location> Locations { get; set; }
        public virtual DbSet<EmploymentLocation> EmploymentLocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.HasDefaultSchema("hr");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("hr"));
            modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new CostCenterConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentStatusConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentTypeConfiguration());
            modelBuilder.ApplyConfiguration(new GradeConfiguration());
            modelBuilder.ApplyConfiguration(new JobLevelConfiguration());
            modelBuilder.ApplyConfiguration(new JobTitleConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new LocationConfiguration());
            modelBuilder.ApplyConfiguration(new EmploymentLocationsConfiguration());

        }
    }

}
