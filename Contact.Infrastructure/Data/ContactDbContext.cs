using Contact.Domain.Entities;
using Contact.Infrastructure.Configurations;
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
namespace Contact.Infrastructure.Data
{


    public class ContactDbContext : Base_DbContext
    {
        public ContactDbContext(
            DbContextOptions<ContactDbContext> options,
            IServiceProvider serviceProvider)
            : base(options, serviceProvider)
        {
        }
        public ContactDbContext(DbContextOptions<ContactDbContext> options)
      : base(options, new ServiceCollection().BuildServiceProvider()) 
        {
        }

        public virtual DbSet<ContactProfile> ContactProfiles { get; set; }
        public virtual DbSet<ContactResource> ContactResources { get; set; }
        public virtual DbSet<ContactProfileAssignment> ContactProfileAssignments { get; set; }
        
        public virtual DbSet<PhoneBookInfoView> PhoneBookInfoView { get; set; }
       
        public override void EnsureTriggers(CancellationToken cancellationToken = default(CancellationToken))
        {
            //EnsureTrigger("HR.Infrastructure.SqlScript", "CreateAssignmentTrigger.sql", "trg_Assignments_CheckOverlap");
        }
        public override void EnsureViews(CancellationToken cancellationToken = default)
        {
            //EnsureView("Contact.Infrastructure.SqlScript", "CreatePhoneBookInfoViewScript.sql", "PhoneBook_Info_View", "contact");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.HasDefaultSchema("contact");

            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration("contact"));
            modelBuilder.ApplyConfiguration(new PhoneBookInfoViewConfiguration());
            modelBuilder.ApplyConfiguration(new ContactResourceConfiguration());
            modelBuilder.ApplyConfiguration(new ContactProfileConfiguration());
            modelBuilder.ApplyConfiguration(new ContactProfileAssignmentConfiguration());

        }
    }

}
