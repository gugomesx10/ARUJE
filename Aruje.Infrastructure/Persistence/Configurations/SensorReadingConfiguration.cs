using Aruje.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aruje.Infrastructure.Persistence.Configurations;

public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
{
    public void Configure(EntityTypeBuilder<SensorReading> builder)
    {
        builder.ToTable("sensor_readings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SensorId)
            .IsRequired();

        builder.Property(x => x.Temperature);

        builder.Property(x => x.AirHumidity);

        builder.Property(x => x.SoilMoisture);

        builder.Property(x => x.Luminosity);

        builder.Property(x => x.ReadingDate)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => x.SensorId);
        builder.HasIndex(x => x.ReadingDate);
    }
}