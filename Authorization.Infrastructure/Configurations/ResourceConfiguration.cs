using Authorization.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Infrastructure.Database.Configurations;

namespace Authorization.Infrastructure.Configurations
{
    public class ResourceConfiguration : DataScopedEntityConfiguration<Resource>
    {
        public override void Configure(EntityTypeBuilder<Resource> builder)
        {
            base.Configure(builder);
            builder.ToTable("Resources", "authorization");

            builder.Property(r => r.Key).IsRequired().HasMaxLength(100).IsUnicode(false);
            builder.HasIndex(r => r.Key).IsUnique();

            // ایندکس روی Path برای کوئری‌های بازگشتی (Hierarchical)
            builder.HasIndex(r => r.ResourcePath);
        }
    }
}
