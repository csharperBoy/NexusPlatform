using Core.Domain.Common;
using Core.Domain.Common.EntityProperties;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Database.Configurations
{
    public class BaseConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : class
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            if (typeof(IAuditableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ConfigureAuditable(builder);
            }

            if (typeof(IOwnerableEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ConfigureDataScoped(builder);
            }

            if (typeof(BaseEntity).IsAssignableFrom(typeof(TEntity)))
            {
                ConfigureBaseEntity(builder);
            }
        }

        private void ConfigureAuditable(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property("CreatedAt")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property("CreatedBy")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property("ModifiedAt")
                .IsRequired(false);

            builder.Property("ModifiedBy")
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex("CreatedAt").HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedAt");
            builder.HasIndex("ModifiedAt").HasDatabaseName($"IX_{typeof(TEntity).Name}_ModifiedAt");
            builder.HasIndex("CreatedBy").HasDatabaseName($"IX_{typeof(TEntity).Name}_CreatedBy");
            builder.HasIndex("ModifiedBy").HasDatabaseName($"IX_{typeof(TEntity).Name}_ModifiedBy");
        }

        private void ConfigureDataScoped(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property("OwnerOrganizationUnitId").IsRequired(false);
            builder.Property("OwnerPersonId").IsRequired(false);

            builder.HasIndex("OwnerOrganizationUnitId").HasDatabaseName($"IX_{typeof(TEntity).Name}_OwnerOrgUnit");
            builder.HasIndex("OwnerPersonId").HasDatabaseName($"IX_{typeof(TEntity).Name}_OwnerPerson");
            builder.HasIndex(new[] { "OwnerOrganizationUnitId", "OwnerPersonId" })
                   .HasDatabaseName($"IX_{typeof(TEntity).Name}_ScopedLookup");
        }

        private void ConfigureBaseEntity(EntityTypeBuilder<TEntity> builder)
        {
            // اگر خاصیتی در BaseEntity داری، اینجا تنظیم کن
        }
    }

}
