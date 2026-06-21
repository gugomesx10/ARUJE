using Aruje.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<SensorReadingCreatedConsumer>();

var host = builder.Build();

host.Run();