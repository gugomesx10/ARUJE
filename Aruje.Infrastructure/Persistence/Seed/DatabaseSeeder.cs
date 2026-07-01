using System.Text.Json;
using Aruje.Application.Messaging;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Aruje.Infrastructure.Persistence.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ArujeDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ArujeDbContext>>();

        await context.Database.MigrateAsync();

        const string demoFarmName = "Fazenda Demo Arujé";

        var demoUsers = new[]
        {
            new
            {
                FullName = "Gustavo Gomes",
                Email = "gustavo@aruje.com",
                Password = "Aruje123@",
                Role = UserRole.Admin
            },
            new
            {
                FullName = "Manager",
                Email = "manager@aruje.com",
                Password = "Aruje123@",
                Role = UserRole.Manager
            },
            new
            {
                FullName = "Operator",
                Email = "operator@aruje.com",
                Password = "Aruje123@",
                Role = UserRole.Operator
            }
        };

        foreach (var demoUser in demoUsers)
        {
            var userAlreadyExists = await context.Users
                .AnyAsync(user => user.Email == demoUser.Email);

            if (userAlreadyExists)
                continue;

            var user = new User(
                demoUser.FullName,
                demoUser.Email,
                BCrypt.Net.BCrypt.HashPassword(demoUser.Password),
                demoUser.Role
            );

            await context.Users.AddAsync(user);
        }

        var demoFarmAlreadyExists = await context.Farms
            .AnyAsync(farm => farm.Name == demoFarmName);

        if (demoFarmAlreadyExists)
        {
            await context.SaveChangesAsync();

            logger.LogInformation("Demo users verified. Demo farm already exists. Skipping farm seed.");
            return;
        }

        var farm = new Farm(
            demoFarmName,
            "Gustavo Gomes",
            "São Paulo - SP",
            120.5
        );

        var crop = new Crop(
            "Plantação de Soja Demo",
            CropType.Soybean,
            40,
            DateTime.UtcNow.AddDays(-45),
            farm.Id
        );

        var soilSensor = new Sensor(
            "Sensor Solo Demo 01",
            SensorType.SoilMoisture,
            "ARUJE-SOIL-DEMO-001",
            crop.Id
        );

        var temperatureSensor = new Sensor(
            "Sensor Temperatura Demo 01",
            SensorType.Temperature,
            "ARUJE-TEMP-DEMO-001",
            crop.Id
        );

        var readings = new List<SensorReading>
        {
            new(
                soilSensor.Id,
                26,
                62,
                48,
                700,
                DateTime.UtcNow.AddMinutes(-40)
            ),
            new(
                temperatureSensor.Id,
                29,
                58,
                45,
                720,
                DateTime.UtcNow.AddMinutes(-30)
            ),
            new(
                soilSensor.Id,
                38.5,
                42,
                18,
                760,
                DateTime.UtcNow.AddMinutes(-20)
            ),
            new(
                temperatureSensor.Id,
                36.8,
                40,
                28,
                810,
                DateTime.UtcNow.AddMinutes(-10)
            )
        };

        var outboxMessages = readings
            .Select(reading =>
            {
                var message = new SensorReadingCreatedMessage(
                    reading.Id,
                    reading.SensorId,
                    reading.Temperature,
                    reading.AirHumidity,
                    reading.SoilMoisture,
                    reading.Luminosity,
                    reading.ReadingDate,
                    reading.CreatedAt
                );

                return new OutboxMessage(
                    nameof(SensorReadingCreatedMessage),
                    JsonSerializer.Serialize(message)
                );
            })
            .ToList();

        await context.Farms.AddAsync(farm);
        await context.Crops.AddAsync(crop);
        await context.Sensors.AddRangeAsync(soilSensor, temperatureSensor);
        await context.SensorReadings.AddRangeAsync(readings);
        await context.OutboxMessages.AddRangeAsync(outboxMessages);

        await context.SaveChangesAsync();

        logger.LogInformation("Demo seed created successfully.");
    }
}