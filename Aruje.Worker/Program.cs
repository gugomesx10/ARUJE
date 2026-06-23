using Aruje.Application.DependencyInjection;
using Aruje.Application.Interfaces.Services;
using Aruje.Infrastructure.DependencyInjection;
using Aruje.Infrastructure.Services;
using Aruje.Worker.Consumers;
using Aruje.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<OutboxPublisherService>();
builder.Services.AddHostedService<SensorReadingCreatedConsumer>();
builder.Services.AddHttpClient<IPushNotificationService, PushNotificationService>();

var host = builder.Build();

host.Run();