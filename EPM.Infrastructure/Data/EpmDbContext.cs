using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using EPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EPM.Infrastructure.Configurations;

namespace EPM.Infrastructure.Data;

public class EpmDbContext : Base_DbContext
{
    public EpmDbContext(
        DbContextOptions<EpmDbContext> options,
        IServiceProvider serviceProvider)
        : base(options, serviceProvider)
    {
    }

    public EpmDbContext(DbContextOptions<EpmDbContext> options)
        : base(options, new ServiceCollection().BuildServiceProvider())
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<MapItem> MapItems { get; set; }     
    public virtual DbSet<LookUp> LookUps { get; set; }
    public virtual DbSet<LookUpDestination> LookUpDestinations { get; set; }
    public virtual DbSet<LookUpValue> LookUpValues { get; set; }
    public virtual DbSet<Indicator> Indicators { get; set; }
    public virtual DbSet<IndicatorCategory> IndicatorCategoryies { get; set; }
    public virtual DbSet<IndicatorValue> IndicatorValues { get; set; }
    public virtual DbSet<ClaimUserOnIndicator> ClaimUserOnIndicators { get; set; }
    public virtual DbSet<Category> Categories { get; set; }
    
    public override void EnsureViews(CancellationToken cancellationToken = default) { }
    public override void EnsureTriggers(CancellationToken cancellationToken = default) { }

    /* ═══════════ Model Creating ═══════════ */

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("epm");

        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("epm"));
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new MapItemConfiguration());
        modelBuilder.ApplyConfiguration(new LookUpConfiguration());
        modelBuilder.ApplyConfiguration(new LookUpDestinationConfiguration());
        modelBuilder.ApplyConfiguration(new LookUpValueConfiguration());
        modelBuilder.ApplyConfiguration(new IndicatorConfiguration());
        modelBuilder.ApplyConfiguration(new IndicatorCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new IndicatorValueConfiguration());
        modelBuilder.ApplyConfiguration(new ClaimUserOnIndicatorConfiguration());
        modelBuilder.ApplyConfiguration(new CategoryConfiguration());

    }
}