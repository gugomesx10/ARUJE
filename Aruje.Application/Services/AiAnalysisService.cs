using Aruje.Application.Interfaces.Services;
using Aruje.Domain.Entities;

namespace Aruje.Application.Services;

public class AiAnalysisService : IAiAnalysisService
{
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
}