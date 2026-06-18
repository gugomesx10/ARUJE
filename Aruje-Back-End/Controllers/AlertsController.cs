using Aruje.Application.DTOs.Alerts;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Aruje.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pela consulta de alertas gerados pelo sistema.
/// </summary>
[ApiController]
[Route("api/alerts")]
[Produces("application/json")]
[SwaggerTag("Consulta de alertas gerados a partir das leituras IoT.")]
public class AlertsController : ControllerBase
{
    private readonly AlertService _alertService;

    public AlertsController(AlertService alertService)
    {
        _alertService = alertService;
    }

    /// <summary>
    /// Lista todos os alertas ativos.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar alertas",
        Description = "Retorna todos os alertas ativos gerados pelo sistema."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<AlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> GetAll()
    {
        var alerts = await _alertService.GetAllAsync();

        if (!alerts.Any())
            return NoContent();

        return Ok(alerts);
    }

    /// <summary>
    /// Lista alertas por status.
    /// </summary>
    [HttpGet("by-status/{status}")]
    [SwaggerOperation(
        Summary = "Listar alertas por status",
        Description = "Retorna alertas filtrados pelo status informado."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<AlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> GetByStatus(
        [FromRoute] AlertStatus status)
    {
        var alerts = await _alertService.GetByStatusAsync(status);

        if (!alerts.Any())
            return NoContent();

        return Ok(alerts);
    }

    /// <summary>
    /// Lista alertas por severidade.
    /// </summary>
    [HttpGet("by-severity/{severity}")]
    [SwaggerOperation(
        Summary = "Listar alertas por severidade",
        Description = "Retorna alertas filtrados pela severidade informada."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<AlertResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> GetBySeverity(
        [FromRoute] AlertSeverity severity)
    {
        var alerts = await _alertService.GetBySeverityAsync(severity);

        if (!alerts.Any())
            return NoContent();

        return Ok(alerts);
    }
}