using Aruje.Application.DTOs.AiAnalyses;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pela consulta de análises geradas pela IA.
/// </summary>
[ApiController]
[Route("api/ai-analyses")]
[Produces("application/json")]
[SwaggerTag("Consulta de análises inteligentes geradas a partir dos alertas.")]
[Authorize(Roles = "Admin,Manager,Operator")]
public class AiAnalysesController : ControllerBase
{
    private readonly AiAnalysisService _aiAnalysisService;

    public AiAnalysesController(AiAnalysisService aiAnalysisService)
    {
        _aiAnalysisService = aiAnalysisService;
    }

    /// <summary>
    /// Lista todas as análises inteligentes ativas.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar análises da IA",
        Description = "Retorna todas as análises inteligentes geradas pelo sistema."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<AiAnalysisResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<AiAnalysisResponse>>> GetAll()
    {
        var analyses = await _aiAnalysisService.GetAllAsync();

        if (!analyses.Any())
            return NoContent();

        return Ok(analyses);
    }

    /// <summary>
    /// Busca uma análise pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar análise por ID",
        Description = "Retorna os dados de uma análise inteligente específica."
    )]
    [ProducesResponseType(typeof(AiAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AiAnalysisResponse>> GetById([FromRoute] Guid id)
    {
        var analysis = await _aiAnalysisService.GetByIdAsync(id);

        return Ok(analysis);
    }

    /// <summary>
    /// Busca uma análise vinculada a um alerta.
    /// </summary>
    [HttpGet("by-alert/{alertId:guid}")]
    [SwaggerOperation(
        Summary = "Buscar análise por alerta",
        Description = "Retorna a análise inteligente vinculada a um alerta específico."
    )]
    [ProducesResponseType(typeof(AiAnalysisResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AiAnalysisResponse>> GetByAlertId(
        [FromRoute] Guid alertId)
    {
        var analysis = await _aiAnalysisService.GetByAlertIdAsync(alertId);

        return Ok(analysis);
    }

    /// <summary>
    /// Remove logicamente uma análise inteligente.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover análise da IA",
        Description = "Realiza a remoção lógica de uma análise inteligente, mantendo o histórico no banco."
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
        await _aiAnalysisService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}