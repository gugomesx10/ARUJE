using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Aruje.Infrastructure.Persistence.Context;

public class ArujeDbContextFactory : IDesignTimeDbContextFactory<ArujeDbContext>
{
    public ArujeDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ARUJE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Environment variable ARUJE_CONNECTION_STRING was not configured.");

        var optionsBuilder = new DbContextOptionsBuilder<ArujeDbContext>();

        optionsBuilder.UseNpgsql(connectionString);

        return new ArujeDbContext(optionsBuilder.Options);
    }
}