using Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Configuration
{
    public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
    {
        public void Configure(EntityTypeBuilder<ApplicationRole> b)
        {
            b.ToTable("AspNetRoles", "identity");

            b.Property(r => r.Description)
                .HasMaxLength(500);

            b.Property(r => r.OrderNum)
                .HasDefaultValue(0);

            b.Property(r => r.CreatedAt)
                .HasDefaultValueSql("getutcdate()");
        }
    }
}
