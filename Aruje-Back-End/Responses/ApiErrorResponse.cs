namespace Aruje_Back_End.Responses;

/// <summary>
/// Modelo padrão para respostas de erro da API.
/// </summary>
public record ApiErrorResponse(
    int StatusCode,
    string Message,
    string? Details = null,
    IReadOnlyCollection<string>? Errors = null
);