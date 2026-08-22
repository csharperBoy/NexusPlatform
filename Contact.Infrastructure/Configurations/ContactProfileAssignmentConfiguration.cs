using Contact.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contact.Infrastructure.Configurations
{
    
    public class ContactProfileAssignmentConfiguration : BaseConfiguration<ContactProfileAssignment>
    {
        public override void Configure(EntityTypeBuilder<ContactProfileAssignment> builder)
        {
            base.Configure(builder);


            builder.ToTable("ContactProfileAssignments", "contact");


        }

    }
}
