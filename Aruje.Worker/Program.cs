using Aruje.Application.DependencyInjection;
using Aruje.Infrastructure.DependencyInjection;
using Aruje.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<SensorReadingCreatedConsumer>();

var host = builder.Build();

host.Run();