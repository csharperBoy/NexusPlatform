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
using HR.IrisaSync.Extention.Entities;
using HR.IrisaSync.Extention.Configurations;
namespace HR.IrisaSync.Extention.Data
{


    public class IrisaExtentionDbContext : Base_DbContext
    {
        public IrisaExtentionDbContext(
            DbContextOptions<IrisaExtentionDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }
        public IrisaExtentionDbContext(DbContextOptions<IrisaExtentionDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) 
        {
        }
        public virtual DbSet<JobTitleMap> JobTitleMap { get; set; }
        public virtual DbSet<JobLevelMap> JobLevelMap { get; set; }
        public virtual DbSet<OrganizationUnitMap> OrganizationUnitMap { get; set; }
        
        //public override void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken))
        //{
        //    //EnsureTrigger("HR.Infrastructure.SqlScript", "CreateAssignmentTrigger.sql", "trg_Assignments_CheckOverlap");
        //}
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.HasDefaultSchema("hr");

            modelBuilder.ApplyConfiguration(new JobTitleMapConfiguration());
            modelBuilder.ApplyConfiguration(new JobLevelMapConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationUnitMapConfiguration());
            

        }
    }

}
