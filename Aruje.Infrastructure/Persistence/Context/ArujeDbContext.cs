using Aruje.Application.Interfaces.Persistence;
using Aruje.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aruje.Infrastructure.Persistence.Context;

public class ArujeDbContext : DbContext, IUnitOfWork
{
    public ArujeDbContext(DbContextOptions<ArujeDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Farm> Farms => Set<Farm>();
    public DbSet<Crop> Crops => Set<Crop>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AiAnalysis> AiAnalyses => Set<AiAnalysis>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ArujeDbContext).Assembly);
    }
}