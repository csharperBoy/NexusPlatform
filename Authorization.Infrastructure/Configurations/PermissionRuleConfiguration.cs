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
    public class PermissionRuleConfiguration : BaseConfiguration<PermissionRule>
    {
        public override void Configure(EntityTypeBuilder<PermissionRule> builder)
        {
            base.Configure(builder);

            builder.ToTable("PermissionRules", "authorization");

            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();

            
        }
    }

    }
