using Authorization.Domain.Entities;
using Authorization.Infrastructure.Configurations;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Data
{
    public class AuthorizationDbContext : BaseDbContext
    {
        public AuthorizationDbContext(
            DbContextOptions<AuthorizationDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }
        public AuthorizationDbContext(DbContextOptions<AuthorizationDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) // ServiceProvider خالی
        {
        }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Permission> Permissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // اعمال اسکیما
            modelBuilder.HasDefaultSchema("authorization");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("authorization"));
            modelBuilder.ApplyConfiguration(new ResourceConfiguration());
            modelBuilder.ApplyConfiguration(new PermissionConfiguration());

        }
    }

}
