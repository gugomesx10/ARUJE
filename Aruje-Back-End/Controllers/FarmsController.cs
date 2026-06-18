using Aruje.Application.DTOs.Farms;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo gerenciamento de fazendas.
/// </summary>
[ApiController]
[Route("api/farms")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de fazendas monitoradas pelo Arujé.")]
public class FarmsController : ControllerBase
{
    private readonly FarmService _farmService;

    public FarmsController(FarmService farmService)
    {
        _farmService = farmService;
    }

    /// <summary>
    /// Cadastra uma nova fazenda.
    /// </summary>
    /// <param name="request">Dados necessários para cadastrar a fazenda.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>Dados da fazenda cadastrada.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastrar fazenda",
        Description = "Cria uma nova fazenda para ser monitorada pela plataforma Arujé."
    )]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FarmResponse>> Create(
        [FromBody] CreateFarmRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var farm = await _farmService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetAll), new { id = farm.Id }, farm);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiErrorResponse(
                StatusCodes.Status400BadRequest,
                ex.Message
            ));
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new ApiErrorResponse(
                StatusCodes.Status409Conflict,
                "Conflito ao cadastrar fazenda.",
                ex.InnerException?.Message
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao cadastrar fazenda.",
                    ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Lista todas as fazendas ativas.
    /// </summary>
    /// <returns>Lista de fazendas cadastradas.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar fazendas",
        Description = "Retorna todas as fazendas ativas cadastradas no sistema."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<FarmResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<FarmResponse>>> GetAll()
    {
        try
        {
            var farms = await _farmService.GetAllAsync();

            if (!farms.Any())
                return NoContent();

            return Ok(farms);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao listar fazendas.",
                    ex.Message
                )
            );
        }
    }
}