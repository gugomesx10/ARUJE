using Aruje.Application.DTOs.Alerts;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Aruje.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pela consulta e gestão de alertas.
/// </summary>
[ApiController]
[Route("api/alerts")]
[Produces("application/json")]
[SwaggerTag("Consulta e gestão de alertas gerados pelas leituras IoT.")]
[Authorize(Roles = "Admin,Manager,Operator")]
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
        Description = "Retorna todos os alertas ativos gerados a partir das leituras IoT."
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
    /// Busca um alerta pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar alerta por ID",
        Description = "Retorna os dados de um alerta específico."
    )]
    [ProducesResponseType(typeof(AlertResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AlertResponse>> GetById([FromRoute] Guid id)
    {
        var alert = await _alertService.GetByIdAsync(id);

        return Ok(alert);
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
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
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AlertResponse>>> GetBySeverity(
        [FromRoute] AlertSeverity severity)
    {
        var alerts = await _alertService.GetBySeverityAsync(severity);

        if (!alerts.Any())
            return NoContent();

        return Ok(alerts);
    }

    /// <summary>
    /// Altera o alerta para em atendimento.
    /// </summary>
    [HttpPatch("{id:guid}/start-processing")]
    [SwaggerOperation(
        Summary = "Iniciar atendimento do alerta",
        Description = "Altera o status do alerta para em atendimento."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> StartProcessing(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _alertService.StartProcessingAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Resolve um alerta.
    /// </summary>
    [HttpPatch("{id:guid}/resolve")]
    [SwaggerOperation(
        Summary = "Resolver alerta",
        Description = "Altera o status do alerta para resolvido."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Resolve(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _alertService.ResolveAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Fecha um alerta.
    /// </summary>
    [HttpPatch("{id:guid}/close")]
    [SwaggerOperation(
        Summary = "Fechar alerta",
        Description = "Altera o status do alerta para fechado."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Close(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _alertService.CloseAsync(id, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Remove logicamente um alerta.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover alerta",
        Description = "Realiza a remoção lógica de um alerta, mantendo o histórico no banco."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _alertService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}