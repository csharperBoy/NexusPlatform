using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample.Domain;
using Sample.Domain.Entities;
namespace Sample.Infrastructure.Data
{
    public class SampleDbContext : DbContext
    {
        public SampleDbContext(DbContextOptions<SampleDbContext> options) : base(options) { }

        public DbSet<SampleEntity> Sample { get; set; } = null!;
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("sample");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("sample"));
            modelBuilder.Entity<SampleEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.property1).IsRequired().HasMaxLength(200);
            });
        }
    }
}
