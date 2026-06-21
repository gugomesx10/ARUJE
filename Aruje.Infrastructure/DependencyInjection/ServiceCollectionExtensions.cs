using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Infrastructure.Persistence.Context;
using Aruje.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Aruje.Infrastructure.Services;
using Aruje.Application.Interfaces.Messaging;
using Aruje.Infrastructure.Messaging;

namespace Aruje.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ArujeDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<ArujeDbContext>());

        services.AddScoped<IOutboxMessageRepository, OutboxMessageRepository>();
        services.AddScoped<IMessagePublisher, RabbitMqMessagePublisher>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IFarmRepository, FarmRepository>();
        services.AddScoped<ICropRepository, CropRepository>();
        services.AddScoped<ISensorRepository, SensorRepository>();
        services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IAiAnalysisRepository, AiAnalysisRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        
        

        return services;
    }
}