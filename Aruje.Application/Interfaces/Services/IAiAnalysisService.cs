using Aruje.Domain.Entities;

namespace Aruje.Application.Interfaces.Services;

public interface IAiAnalysisService
{
    Task<AiAnalysis> GenerateAnalysisAsync(
        Alert alert,
        SensorReading reading,
        CancellationToken cancellationToken = default);
}