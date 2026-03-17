using Authorization.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Infrastructure.Configurations
{
    public class JoinDetailConfiguration : BaseConfiguration<JoinDetail>
    {
        public override void Configure(EntityTypeBuilder<JoinDetail> builder)
        {
            base.Configure(builder);

            builder.ToTable("JoinDetail", "authorization");

            builder.HasKey(ds => ds.Id);
            builder.HasIndex(ds => ds.Id).IsUnique();


        }
    }
}
