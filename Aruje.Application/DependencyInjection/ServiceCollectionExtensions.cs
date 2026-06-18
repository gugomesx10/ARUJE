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
        services.AddScoped<UserService>();
        services.AddScoped<IIoTIngestionService, SensorReadingService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IAiAnalysisService, AiAnalysisService>();

        return services;
    }
}