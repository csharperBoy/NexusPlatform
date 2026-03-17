using Authorization.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Configurations
{
    public class ScopeConfiguration : BaseConfiguration<Resource>
    {
        public override void Configure(EntityTypeBuilder<Resource> builder)
        {
            base.Configure(builder);
            builder.ToTable("Scope", "authorization");

            builder.Property(r => r.Key).IsRequired().HasMaxLength(100).IsUnicode(false);
            builder.HasIndex(r => r.Key).IsUnique();

            // ایندکس روی Path برای کوئری‌های بازگشتی (Hierarchical)
            builder.HasIndex(r => r.ResourcePath);
        }
    }
}
