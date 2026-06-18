using Aruje.Application.DTOs.AiAnalyses;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pela consulta de análises geradas pela IA.
/// </summary>
[ApiController]
[Route("api/ai-analyses")]
[Produces("application/json")]
[SwaggerTag("Consulta de análises inteligentes geradas a partir dos alertas.")]
public class AiAnalysesController : ControllerBase
{
    private readonly AiAnalysisService _aiAnalysisService;

    public AiAnalysesController(AiAnalysisService aiAnalysisService)
    {
        _aiAnalysisService = aiAnalysisService;
    }

    /// <summary>
    /// Lista todas as análises inteligentes.
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
    /// Busca análise inteligente por alerta.
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

        if (analysis is null)
            return NotFound(new ApiErrorResponse(
                StatusCodes.Status404NotFound,
                "AI analysis not found for this alert."
            ));

        return Ok(analysis);
    }
}