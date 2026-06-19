using Aruje.Application.Interfaces.Services;
using Aruje.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Aruje.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);

        services.AddScoped<FarmService>();
        services.AddScoped<CropService>();
        services.AddScoped<SensorService>();
        services.AddScoped<SensorReadingService>();
        services.AddScoped<AlertService>();
        services.AddScoped<AiAnalysisService>();
        services.AddScoped<UserService>();
        services.AddScoped<IIoTIngestionService>(provider =>
            provider.GetRequiredService<SensorReadingService>());
        services.AddScoped<IAlertService>(provider =>
            provider.GetRequiredService<AlertService>());
        services.AddScoped<IAiAnalysisService>(provider =>
            provider.GetRequiredService<AiAnalysisService>());

        return services;
    }
}