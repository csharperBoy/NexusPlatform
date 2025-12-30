using Core.Domain.Common;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrganizationManagement.Domain.Entities;
using OrganizationManagement.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace OrganizationManagement.Infrastructure.Data
{


    public class OrganizationManagementDbContext : BaseDbContext
    {
        public OrganizationManagementDbContext(
            DbContextOptions<OrganizationManagementDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }
        public OrganizationManagementDbContext(DbContextOptions<OrganizationManagementDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) 
        {
        }
        public DbSet<Assignment> Assignment { get; set; }
        public DbSet<OrganizationUnit> OrganizationUnit { get; set; }
        public DbSet<Position> Position { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // اعمال اسکیما
            modelBuilder.HasDefaultSchema("organization");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("organization"));
            modelBuilder.ApplyConfiguration(new AssignmentConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationUnitConfiguration());
            modelBuilder.ApplyConfiguration(new PositionConfiguration());

        }
    }

}
