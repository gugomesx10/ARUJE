using Aruje.Domain.Common;

namespace Aruje.Domain.Entities;

public class AiAnalysis : BaseEntity
{
    public Guid AlertId { get; private set; }

    public string RiskLevel { get; private set; }
    public string Reason { get; private set; }
    public string Recommendation { get; private set; }
    public string Provider { get; private set; }

    private AiAnalysis()
    {
        RiskLevel = string.Empty;
        Reason = string.Empty;
        Recommendation = string.Empty;
        Provider = string.Empty;
    }

    public AiAnalysis(
        Guid alertId,
        string riskLevel,
        string reason,
        string recommendation,
        string provider)
    {
        Validate(alertId, riskLevel, reason, recommendation, provider);

        AlertId = alertId;
        RiskLevel = riskLevel;
        Reason = reason;
        Recommendation = recommendation;
        Provider = provider;
    }

    private static void Validate(
        Guid alertId,
        string riskLevel,
        string reason,
        string recommendation,
        string provider)
    {
        if (alertId == Guid.Empty)
            throw new ArgumentException("AlertId is required.");

        if (string.IsNullOrWhiteSpace(riskLevel))
            throw new ArgumentException("Risk level is required.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.");

        if (string.IsNullOrWhiteSpace(recommendation))
            throw new ArgumentException("Recommendation is required.");

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("AI provider is required.");
    }
}