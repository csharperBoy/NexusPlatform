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
    public class ScopeConfiguration : BaseConfiguration<Scope>
    {
        public override void Configure(EntityTypeBuilder<Scope> builder)
        {
            base.Configure(builder);
            builder.ToTable("Scopes", "authorization");

            // ایندکس روی Path برای کوئری‌های بازگشتی (Hierarchical)
            builder.HasIndex(r => r.PermissionId);
        }
    }
}
