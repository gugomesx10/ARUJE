using Aruje.Application.DTOs.AiAnalyses;
using Aruje.Application.Exceptions;
using Aruje.Application.Interfaces.Persistence;
using Aruje.Application.Interfaces.Repositories;
using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class AiAnalysisService : IAiAnalysisService
{
    private readonly IAiAnalysisRepository _aiAnalysisRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AiAnalysisService(
        IAiAnalysisRepository aiAnalysisRepository,
        IUnitOfWork unitOfWork)
    {
        _aiAnalysisRepository = aiAnalysisRepository;
        _unitOfWork = unitOfWork;
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

        return analyses.Select(ToResponse).ToList();
    }

    public async Task<AiAnalysisResponse> GetByIdAsync(Guid id)
    {
        var analysis = await _aiAnalysisRepository.GetByIdAsync(id);

        if (analysis is null || !analysis.IsActive)
            throw new NotFoundException("AI analysis not found.");

        return ToResponse(analysis);
    }

    public async Task<AiAnalysisResponse> GetByAlertIdAsync(Guid alertId)
    {
        var analysis = await _aiAnalysisRepository.GetByAlertIdAsync(alertId);

        if (analysis is null || !analysis.IsActive)
            throw new NotFoundException("AI analysis not found for this alert.");

        return ToResponse(analysis);
    }

    public async Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var analysis = await _aiAnalysisRepository.GetByIdAsync(id);

        if (analysis is null || !analysis.IsActive)
            throw new NotFoundException("AI analysis not found.");

        await _aiAnalysisRepository.DeleteAsync(analysis);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AiAnalysisResponse ToResponse(AiAnalysis analysis)
    {
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