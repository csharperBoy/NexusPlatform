using Base.Domain.Entities;
using Core.Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Infrastructure.Configurations
{
    public class MenuConfiguration : BaseConfiguration<Menu>
    {
        public override void Configure(EntityTypeBuilder<Menu> builder)
        {
            base.Configure(builder);
            builder.ToTable("Menu", "base");

            builder.Property(r => r.Key).IsRequired().HasMaxLength(100).IsUnicode(false);
            builder.HasIndex(r => r.Key).IsUnique();
        }
    }
}
