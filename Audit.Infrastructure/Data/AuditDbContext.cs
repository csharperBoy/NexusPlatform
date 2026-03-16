using Audit.Domain.Entities;
using Audit.Infrastructure.Configurations;
using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Core.Domain.Interfaces;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Data
{
    public class AuditDbContext : BaseDbContext 
    {
        public AuditDbContext(
        DbContextOptions<AuditDbContext> options,
        IServiceProvider serviceProvider)
        : base(options, serviceProvider)
        {
        }
        public AuditDbContext(DbContextOptions<AuditDbContext> options)
       : base(options
             , new ServiceCollection().BuildServiceProvider()
             ) {
        }
        
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("audit");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("audit"));
            modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
            //modelBuilder.ApplyConfiguration(Configure<AuditLog>());
            /*modelBuilder.Entity<AuditLog>(entity =>
            { 
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EntityName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(100);
            });*/
        }

    }
}
