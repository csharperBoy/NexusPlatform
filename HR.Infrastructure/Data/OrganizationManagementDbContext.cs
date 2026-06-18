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
        public DbSet<OrganizationUnit> OrganizationUnit { get; set; }
        public DbSet<Post> Position { get; set; }

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
