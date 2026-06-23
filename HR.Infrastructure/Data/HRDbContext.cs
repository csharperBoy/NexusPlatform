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
        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<CostCenter> CostCenter { get; set; }
        public DbSet<Employment> Employment { get; set; }
        public DbSet<EmploymentStatus> EmploymentStatus { get; set; }
        public DbSet<EmploymentType> EmploymentType { get; set; }
        public DbSet<Grade> Grade { get; set; }
        public DbSet<JobLevel> JobLevel { get; set; }
        public DbSet<JobTitle> JobTitle { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnit { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<EmploymentLocations> EmploymentLocations { get; set; }

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
