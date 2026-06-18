using Aruje.Application.DTOs.AiAnalyses;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private readonly IAiAnalysisRepository _aiAnalysisRepository;

    public AiAnalysisService(IAiAnalysisRepository aiAnalysisRepository)
    {
        _aiAnalysisRepository = aiAnalysisRepository;
    }

    public Task<AiAnalysis> GenerateAnalysisAsync(
        Alert alert,
        SensorReading reading,
        CancellationToken cancellationToken = default)
    {
        var analysis = new AiAnalysis(
            alert.Id,
            alert.Severity.ToString(),
            alert.Description,
            "Verificar a plantação e avaliar necessidade de irrigação ou intervenção manual.",
            "RuleBased-Mock"
        );

        return Task.FromResult(analysis);
    }

    public async Task<IReadOnlyList<AiAnalysisResponse>> GetAllAsync()
    {
        var analyses = await _aiAnalysisRepository.GetAllAsync();

        return analyses.Select(analysis => new AiAnalysisResponse(
            analysis.Id,
            analysis.AlertId,
            analysis.RiskLevel,
            analysis.Reason,
            analysis.Recommendation,
            analysis.Provider,
            analysis.CreatedAt
        )).ToList();
    }

    public async Task<AiAnalysisResponse?> GetByAlertIdAsync(Guid alertId)
    {
        var analysis = await _aiAnalysisRepository.GetByAlertIdAsync(alertId);

        if (analysis is null)
            return null;

        return new AiAnalysisResponse(
            analysis.Id,
            analysis.AlertId,
            analysis.RiskLevel,
            analysis.Reason,
            analysis.Recommendation,
            analysis.Provider,
            analysis.CreatedAt
        );
    }
}