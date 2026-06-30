using Core.Domain.Common;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
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

        public DbSet<Party> Parties { get; set; } = null!;
        public DbSet<PartiesRelation> PartiesRelations { get; set; } = null!;
        public DbSet<NaturalPerson> naturalPerson { get; set; } = null!;
        public DbSet<LegalPerson> legalPersons { get; set; } = null!;
        public DbSet<NaturalPersonProfile> PersonProfiles { get; set; } = null!;
        public DbSet<PartyContact> PersonContacts { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("people");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("people"));
            modelBuilder.ApplyConfiguration(new LegalPersonConfiguration());
            modelBuilder.ApplyConfiguration(new NaturalPersonConfiguration());
            modelBuilder.ApplyConfiguration(new PartyConfiguration());
            modelBuilder.ApplyConfiguration(new PartiesRelationConfiguration());
            modelBuilder.ApplyConfiguration(new NaturalPersonProfileConfiguration());
            modelBuilder.ApplyConfiguration(new PartyContactConfiguration());
            
        }

    }
}
