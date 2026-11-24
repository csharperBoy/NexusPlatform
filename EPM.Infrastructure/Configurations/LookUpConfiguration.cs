using Core.Infrastructure.Database.Configurations;
using EPM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPM.Infrastructure.Configurations
{

    public class LookUpConfiguration : BaseConfiguration<LookUp>
    {
        public override void Configure(EntityTypeBuilder<LookUp> entity)
        {
            base.Configure(entity);
            entity.ToTable("LookUp", "epm");
        }
    }
}