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
    public class PersonDbContextFactory : IDesignTimeDbContextFactory<PersonDbContext>
    {
        public PersonDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<PersonDbContext>();
            optionsBuilder.UseSqlServer(conn, b =>
            {
                b.MigrationsAssembly(typeof(PersonDbContext).Assembly.GetName().Name);
                b.MigrationsHistoryTable("__PersonMigrationsHistory", "person");
            });

            return new PersonDbContext(optionsBuilder.Options);
        }
    }
}
