using Aruje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aruje.Infrastructure.Persistence.Configurations;

public class AiAnalysisConfiguration : IEntityTypeConfiguration<AiAnalysis>
{
    public void Configure(EntityTypeBuilder<AiAnalysis> builder)
    {
        builder.ToTable("ai_analyses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AlertId)
            .IsRequired();

        builder.Property(x => x.RiskLevel)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.Reason)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Recommendation)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(80);

        builder.HasIndex(x => x.AlertId);
    }
}