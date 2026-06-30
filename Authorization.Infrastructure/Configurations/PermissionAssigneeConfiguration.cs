using Authorization.Domain.Entities;
using Core.Domain.Interfaces;
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
    public class PermissionAssigneeConfiguration : BaseConfiguration<PermissionAssignee>
    {
        public override void Configure(EntityTypeBuilder<PermissionAssignee> builder)
        {
            base.Configure(builder); // اعمال CreatedAt و...

            builder.ToTable("PermissionAssignee", "authorization");

        }
    }

}
