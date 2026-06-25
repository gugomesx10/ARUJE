using System.Globalization;
using System.Text;
using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;
using Aruje.Domain.Enums;

namespace Aruje.Application.Services.Rag;

public class RagContextBuilder : IRagContextBuilder
{
    private readonly ISensorReadingRepository _sensorReadingRepository;
    private readonly IAlertRepository _alertRepository;
    private readonly IAiAnalysisRepository _aiAnalysisRepository;

    public RagContextBuilder(
        ISensorReadingRepository sensorReadingRepository,
        IAlertRepository alertRepository,
        IAiAnalysisRepository aiAnalysisRepository)
    {
        _sensorReadingRepository = sensorReadingRepository;
        _alertRepository = alertRepository;
        _aiAnalysisRepository = aiAnalysisRepository;
    }

    public async Task<RagContext> BuildAsync(
        string question,
        int maxItems,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuestion = Normalize(question);
        var limit = Math.Clamp(maxItems, 5, 20);

        var readings = await _sensorReadingRepository.GetAllAsync();
        var alerts = await _alertRepository.GetAllAsync();
        var analyses = await _aiAnalysisRepository.GetAllAsync();

        var contextItems = new List<RagContextItem>();

        contextItems.AddRange(
            BuildReadingContextItems(
                readings.Where(x => x.IsActive),
                normalizedQuestion));

        contextItems.AddRange(
            BuildAlertContextItems(
                alerts.Where(x => x.IsActive),
                normalizedQuestion));

        contextItems.AddRange(
            BuildAnalysisContextItems(
                analyses.Where(x => x.IsActive),
                normalizedQuestion));

        var selectedItems = contextItems
            .OrderByDescending(x => x.RelevanceScore)
            .ThenByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToList();

        return new RagContext(
            question.Trim(),
            selectedItems,
            DateTime.UtcNow);
    }

    private static IEnumerable<RagContextItem> BuildReadingContextItems(
        IEnumerable<SensorReading> readings,
        string normalizedQuestion)
    {
        return readings
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .Select(reading =>
            {
                var score = CalculateReadingScore(reading, normalizedQuestion);

                var content =
                    $"Leitura IoT registrada em {reading.CreatedAt:dd/MM/yyyy HH:mm}. " +
                    $"Temperatura: {reading.Temperature:F1}°C. " +
                    $"Umidade do ar: {reading.AirHumidity:F1}%. " +
                    $"Umidade do solo: {reading.SoilMoisture:F1}%. " +
                    $"Luminosidade: {reading.Luminosity:F0} lux. " +
                    $"SensorId: {reading.SensorId}.";

                return new RagContextItem(
                    "SensorReading",
                    reading.Id,
                    "Leitura recente de sensor",
                    content,
                    score,
                    reading.CreatedAt);
            });
    }

    private static IEnumerable<RagContextItem> BuildAlertContextItems(
        IEnumerable<Alert> alerts,
        string normalizedQuestion)
    {
        return alerts
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .Select(alert =>
            {
                var score = CalculateAlertScore(alert, normalizedQuestion);

                var content =
                    $"Alerta registrado em {alert.CreatedAt:dd/MM/yyyy HH:mm}. " +
                    $"Título: {alert.Title}. " +
                    $"Descrição: {alert.Description}. " +
                    $"Severidade: {alert.Severity}. " +
                    $"Status: {alert.Status}. " +
                    $"SensorReadingId: {alert.SensorReadingId}.";

                return new RagContextItem(
                    "Alert",
                    alert.Id,
                    alert.Title,
                    content,
                    score,
                    alert.CreatedAt);
            });
    }

    private static IEnumerable<RagContextItem> BuildAnalysisContextItems(
        IEnumerable<AiAnalysis> analyses,
        string normalizedQuestion)
    {
        return analyses
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .Select(analysis =>
            {
                var score = CalculateAnalysisScore(analysis, normalizedQuestion);

                var content =
                    $"Análise inteligente gerada em {analysis.CreatedAt:dd/MM/yyyy HH:mm}. " +
                    $"Nível de risco: {analysis.RiskLevel}. " +
                    $"Motivo: {analysis.Reason}. " +
                    $"Recomendação: {analysis.Recommendation}. " +
                    $"Provider: {analysis.Provider}. " +
                    $"AlertId: {analysis.AlertId}.";

                return new RagContextItem(
                    "AiAnalysis",
                    analysis.Id,
                    $"Análise IA - Risco {analysis.RiskLevel}",
                    content,
                    score,
                    analysis.CreatedAt);
            });
    }

    private static decimal CalculateReadingScore(
        SensorReading reading,
        string normalizedQuestion)
    {
        decimal score = 1;

        if (reading.Temperature >= 38)
            score += 3;

        if (reading.Temperature >= 35)
            score += 2;

        if (reading.SoilMoisture <= 20)
            score += 3;

        if (reading.SoilMoisture <= 25)
            score += 2;

        if (reading.AirHumidity <= 30)
            score += 1;

        if (reading.Luminosity >= 900)
            score += 1;

        if (ContainsAny(
                normalizedQuestion,
                "temperatura",
                "calor",
                "quente",
                "estresse termico"))
        {
            score += reading.Temperature >= 35 ? 3 : 1;
        }

        if (ContainsAny(
                normalizedQuestion,
                "umidade",
                "solo",
                "irrigacao",
                "agua",
                "seca"))
        {
            score += reading.SoilMoisture <= 25 ? 3 : 1;
        }

        if (ContainsAny(
                normalizedQuestion,
                "sensor",
                "leitura",
                "iot",
                "monitoramento"))
        {
            score += 2;
        }

        score += CalculateRecencyScore(reading.CreatedAt);

        return score;
    }

    private static decimal CalculateAlertScore(
        Alert alert,
        string normalizedQuestion)
    {
        decimal score = 2;

        if (alert.Severity == AlertSeverity.High)
            score += 5;

        if (alert.Severity == AlertSeverity.Medium)
            score += 3;

        if (alert.Status == AlertStatus.Open)
            score += 2;

        var searchableText = Normalize(
            $"{alert.Title} {alert.Description} {alert.Severity} {alert.Status}");

        if (ContainsAny(
                normalizedQuestion,
                "alerta",
                "risco",
                "problema",
                "perigo",
                "critico"))
        {
            score += 4;
        }

        if (HasTermMatch(normalizedQuestion, searchableText))
            score += 3;

        score += CalculateRecencyScore(alert.CreatedAt);

        return score;
    }

    private static decimal CalculateAnalysisScore(
        AiAnalysis analysis,
        string normalizedQuestion)
    {
        decimal score = 2;

        var searchableText = Normalize(
            $"{analysis.RiskLevel} {analysis.Reason} {analysis.Recommendation} {analysis.Provider}");

        if (analysis.RiskLevel.Contains("High", StringComparison.OrdinalIgnoreCase) ||
            analysis.RiskLevel.Contains("Alto", StringComparison.OrdinalIgnoreCase))
        {
            score += 4;
        }

        if (ContainsAny(
                normalizedQuestion,
                "analise",
                "ia",
                "recomendacao",
                "recomenda",
                "fazer",
                "acao"))
        {
            score += 4;
        }

        if (HasTermMatch(normalizedQuestion, searchableText))
            score += 3;

        score += CalculateRecencyScore(analysis.CreatedAt);

        return score;
    }

    private static decimal CalculateRecencyScore(DateTime createdAt)
    {
        var age = DateTime.UtcNow - createdAt;

        if (age.TotalHours <= 1)
            return 3;

        if (age.TotalHours <= 6)
            return 2;

        if (age.TotalDays <= 1)
            return 1;

        return 0;
    }

    private static bool ContainsAny(
        string text,
        params string[] terms)
    {
        return terms.Any(term => text.Contains(Normalize(term)));
    }

    private static bool HasTermMatch(
        string question,
        string targetText)
    {
        var questionTerms = question
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 4)
            .Distinct()
            .ToList();

        return questionTerms.Any(targetText.Contains);
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }
}