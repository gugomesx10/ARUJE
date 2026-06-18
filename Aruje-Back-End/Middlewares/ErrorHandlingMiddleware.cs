using System.Text.Json;
using Aruje.Application.Exceptions;
using Aruje_Back_End.Responses;
using AppValidationException = Aruje.Application.Exceptions.ValidationException;

namespace Aruje_Back_End.Middlewares;

/// <summary>
/// Middleware responsável por padronizar o tratamento de erros da API.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status404NotFound,
                ex.Message
            );
        }
        catch (ConflictException ex)
        {
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status409Conflict,
                ex.Message
            );
        }
        catch (AppValidationException ex)
        {
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message,
                errors: ex.Errors
            );
        }
        catch (ArgumentException ex)
        {
            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred.");

            await WriteErrorResponseAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Unexpected internal server error.",
                _environment.IsDevelopment() ? ex.Message : null
            );
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        int statusCode,
        string message,
        string? details = null,
        IReadOnlyCollection<string>? errors = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ApiErrorResponse(
            statusCode,
            message,
            details,
            errors
        );

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}