using Aruje.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Aruje_Back_End.HealthChecks;

/// <summary>
/// Health check responsável por verificar a conexão com o banco de dados.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ArujeDbContext _context;

    public DatabaseHealthCheck(ArujeDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
                return HealthCheckResult.Healthy("Database connection is healthy.");

            return HealthCheckResult.Unhealthy("Database connection failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed.",
                ex
            );
        }
    }
}