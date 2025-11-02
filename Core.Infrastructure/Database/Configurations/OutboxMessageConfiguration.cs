using Core.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Infrastructure.Database.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        private readonly string _schema;

        public OutboxMessageConfiguration(string schema)
        {
            _schema = schema;
        }

        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages", _schema);
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TypeName)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.AssemblyQualifiedName)
                .HasMaxLength(1024)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.OccurredOnUtc)
                .IsRequired();

            builder.Property(x => x.ProcessedOnUtc);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.RetryCount)
                .IsRequired();

            builder.Property(x => x.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(x => x.ErrorStackTrace)
                .HasMaxLength(2000);
            // Concurrency token
            builder.Property(x => x.RowVersion)
                   .IsRowVersion()
                   .IsConcurrencyToken();

            builder.HasIndex(x => new { x.Status, x.OccurredOnUtc });
            builder.HasIndex(x => x.ProcessedOnUtc);
            builder.HasIndex(x => x.TypeName);
        }
    }
}