using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using People.Domain.Entities;
using People.Infrastructure.Data.Configurations;

namespace People.Infrastructure.Data;

public class PeopleDbContext : DbContext
{
    public PeopleDbContext(DbContextOptions<PeopleDbContext> options) : base(options)
    {
    }

    public DbSet<Individual> Individuals { get; set; }
    public DbSet<IndividualDetail> IndividualDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // اعمال کانفیگ‌ها
        modelBuilder.ApplyConfiguration(new IndividualConfiguration());
        modelBuilder.ApplyConfiguration(new IndividualDetailConfiguration());

        // تنظیم schema پیش‌فرض
        modelBuilder.HasDefaultSchema("people");
    }
}
