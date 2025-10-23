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

            builder.Property(x => x.Type)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.Content)
                .IsRequired();

            builder.Property(x => x.OccurredOn)
                .IsRequired();

            builder.Property(x => x.ProcessedOn);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.RetryCount)
                .IsRequired();

            builder.Property(x => x.Error)
                .HasMaxLength(2000);

            // Indexes for performance
            builder.HasIndex(x => new { x.Status, x.OccurredOn });
            builder.HasIndex(x => x.ProcessedOn);
            builder.HasIndex(x => x.Type);
        }
    }
}
