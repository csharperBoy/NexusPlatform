using Authorization.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Configurations
{
    public class PermissionRuleConfiguration : BaseConfiguration<PermissionRule>
    {
        public override void Configure(EntityTypeBuilder<PermissionRule> builder)
        {
            base.Configure(builder);

            builder.ToTable("PermissionRules", "authorization");

            //relation
            builder.HasOne(pr => pr.JoinDetail)
                   .WithOne(jd => jd.PermissionRule)
                   .HasForeignKey<JoinDetail>(jd => jd.PermissionRuleId)
                   .OnDelete(DeleteBehavior.Cascade); // یا Cascade اگر می‌خواهید JoinDetail هم حذف شود


            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();

            
        }
    }

    }
