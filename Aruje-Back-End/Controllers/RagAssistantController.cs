using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo assistente inteligente com RAG contextual do Arujé.
/// </summary>
[ApiController]
[Route("api/rag")]
[Produces("application/json")]
[SwaggerTag("Assistente inteligente que responde perguntas usando dados reais de leituras, alertas e análises IA.")]
[Authorize(Roles = "Admin,Manager,Operator")]
public class RagAssistantController : ControllerBase
{
    private readonly IRagAssistantService _ragAssistantService;

    public RagAssistantController(IRagAssistantService ragAssistantService)
    {
        _ragAssistantService = ragAssistantService;
    }

    /// <summary>
    /// Envia uma pergunta para o assistente RAG do Arujé.
    /// </summary>
    [HttpPost("ask")]
    [SwaggerOperation(
        Summary = "Perguntar ao assistente RAG",
        Description = "Recebe uma pergunta do usuário e responde com base no contexto recuperado do banco de dados, incluindo leituras IoT, alertas e análises inteligentes."
    )]
    [ProducesResponseType(typeof(RagAskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RagAskResponse>> Ask(
        [FromBody] RagAskRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _ragAssistantService.AskAsync(
            request,
            cancellationToken
        );

        return Ok(response);
    }
}