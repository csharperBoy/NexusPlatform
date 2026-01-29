using Core.Domain.Common;
using Core.Infrastructure.Data;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trader.Server.Collector.Domain;
using Trader.Server.Collector.Domain.Entities;
using Trader.Server.Collector.Infrastructure.Configurations;
namespace Trader.Server.Collector.Infrastructure.Data
{
    public class TraderDbContext : BaseDbContext
    {
        public TraderDbContext(
             DbContextOptions<TraderDbContext> options,
             IServiceProvider serviceProvider)
             : base(options, serviceProvider)
        {
        }
        public TraderDbContext(DbContextOptions<TraderDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) 
        {
        }
        public DbSet<Stock> Stock { get; set; }
        public DbSet<Option> Option { get; set; }
        public DbSet<OptionContract> OptionContract { get; set; }
        public DbSet<SnapShotFromStockTrading> SnapShotFromStockTrading { get; set; }
        public DbSet<SnapShotFromOptionTrading> SnapShotFromOptionTrading { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("trader");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("trader"));

            modelBuilder.ApplyConfiguration(new StockConfiguration());
            modelBuilder.ApplyConfiguration(new OptionConfiguration());
            modelBuilder.ApplyConfiguration(new OptionContractConfiguration());
            modelBuilder.ApplyConfiguration(new SnapShotFromOptionTradingConfiguration());
            modelBuilder.ApplyConfiguration(new SnapShotFromStockTradingConfiguration());

        }
    }
}
