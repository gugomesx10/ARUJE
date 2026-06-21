using Aruje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aruje.Infrastructure.Persistence.Configurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(message => message.Id);

        builder.Property(message => message.EventType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(message => message.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(message => message.OccurredAt)
            .IsRequired();

        builder.Property(message => message.ProcessedAt);

        builder.Property(message => message.Error)
            .HasMaxLength(2000);

        builder.Property(message => message.RetryCount)
            .IsRequired();

        builder.Ignore(message => message.IsProcessed);
    }
}