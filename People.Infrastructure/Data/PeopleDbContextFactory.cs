using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace People.Infrastructure.Data
{
    public class PeopleDbContextFactory : IDesignTimeDbContextFactory<PeopleDbContext>
    {
        public PeopleDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PeopleDbContext>();
            optionsBuilder.UseSqlServer(conn, b =>
            {
                b.MigrationsAssembly(typeof(PeopleDbContext).Assembly.GetName().Name);
                b.MigrationsHistoryTable("__PeopleMigrationsHistory", "people");
            });

            return new PeopleDbContext(optionsBuilder.Options);
        }
    }
}
