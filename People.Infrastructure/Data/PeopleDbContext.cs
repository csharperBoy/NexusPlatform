using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using People.Domain.Entities;
using People.Domain.Entities;
using People.Infrastructure.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace People.Infrastructure.Data
{
    public class PeopleDbContext : DbContext
    {
        public PeopleDbContext(DbContextOptions<PeopleDbContext> options) : base(options) { }
        public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

        public DbSet<Parties> Parties { get; set; } = null!;
        public DbSet<naturalPerson> naturalPerson { get; set; } = null!;
        public DbSet<legalPersons> legalPersons { get; set; } = null!;
        public DbSet<PersonProfile> PersonProfiles { get; set; } = null!;
        public DbSet<PersonContact> PersonContacts { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("people");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("people"));
            modelBuilder.ApplyConfiguration(new LegalPersonConfiguration());
            modelBuilder.ApplyConfiguration(new PartiesConfiguration());
            modelBuilder.ApplyConfiguration(new NaturalPersonConfiguration());
            modelBuilder.ApplyConfiguration(new PersonProfileConfiguration());
            modelBuilder.ApplyConfiguration(new PersonContactConfiguration());
            
        }

    }
}
