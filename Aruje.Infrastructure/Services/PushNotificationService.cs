using System.Net.Http.Json;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Aruje.Infrastructure.Services;

public class PushNotificationService : IPushNotificationService
{
    private readonly ArujeDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly ILogger<PushNotificationService> _logger;

    public PushNotificationService(
        ArujeDbContext context,
        HttpClient httpClient,
        ILogger<PushNotificationService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendAlertNotificationAsync(
        Alert alert,
        CancellationToken cancellationToken = default)
    {
        var tokens = await _context.PushTokens
            .Where(x => x.IsActive)
            .Select(x => x.Token)
            .ToListAsync(cancellationToken);

        if (tokens.Count == 0)
        {
            _logger.LogInformation("Nenhum push token ativo encontrado para envio de alerta.");
            return;
        }

        foreach (var token in tokens)
        {
            var payload = new
            {
                to = token,
                title = alert.Title,
                body = alert.Description,
                sound = "default",
                data = new
                {
                    screen = "AlertDetails",
                    alertId = alert.Id.ToString(),
                    severity = alert.Severity.ToString(),
                }
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://exp.host/--/api/v2/push/send",
                payload,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);

                _logger.LogWarning(
                    "Erro ao enviar push notification. Status: {StatusCode}. Erro: {Error}",
                    response.StatusCode,
                    error);
            }
        }
    }
}