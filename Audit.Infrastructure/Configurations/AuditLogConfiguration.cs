using Audit.Domain.Entities;
using Core.Domain.Interfaces;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Audit.Infrastructure.Configurations
{
    public class AuditLogConfiguration : BaseConfiguration<AuditLog>
     {
         public override void Configure(EntityTypeBuilder<AuditLog> builder)
         {
             base.Configure(builder); // اعمال CreatedAt و...

             builder.ToTable("AuditLogs", "audit");

             builder.HasKey(e => e.Id);
             builder.Property(e => e.Action).IsRequired().HasMaxLength(200);
             builder.Property(e => e.EntityName).IsRequired().HasMaxLength(200);
             builder.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
             builder.Property(e => e.UserId).HasMaxLength(100);


         }
     }
 

    public class AuditLogConfiguration2 : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {            
            builder.ToTable("AuditLogs", "audit");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Action).IsRequired().HasMaxLength(200);
            builder.Property(e => e.EntityName).IsRequired().HasMaxLength(200);
            builder.Property(e => e.EntityId).IsRequired().HasMaxLength(100);
            builder.Property(e => e.UserId).HasMaxLength(100);

        }
    }
}
