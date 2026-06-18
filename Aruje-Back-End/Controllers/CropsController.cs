using Aruje.Application.DTOs.Crops;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo gerenciamento de plantações.
/// </summary>
[ApiController]
[Route("api/crops")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de plantações vinculadas às fazendas.")]
public class CropsController : ControllerBase
{
    private readonly CropService _cropService;

    public CropsController(CropService cropService)
    {
        _cropService = cropService;
    }

    /// <summary>
    /// Cadastra uma nova plantação.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastrar plantação",
        Description = "Cria uma nova plantação vinculada a uma fazenda existente."
    )]
    [ProducesResponseType(typeof(CropResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CropResponse>> Create(
        [FromBody] CreateCropRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var crop = await _cropService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetByFarmId), new { farmId = crop.FarmId }, crop);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Farm not found"))
        {
            return NotFound(new ApiErrorResponse(
                StatusCodes.Status404NotFound,
                ex.Message
            ));
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
                "Conflito ao cadastrar plantação.",
                ex.InnerException?.Message
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao cadastrar plantação.",
                    ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Lista plantações de uma fazenda.
    /// </summary>
    [HttpGet("by-farm/{farmId:guid}")]
    [SwaggerOperation(
        Summary = "Listar plantações por fazenda",
        Description = "Retorna todas as plantações ativas vinculadas a uma fazenda específica."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<CropResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<CropResponse>>> GetByFarmId([FromRoute] Guid farmId)
    {
        try
        {
            var crops = await _cropService.GetByFarmIdAsync(farmId);

            if (!crops.Any())
                return NoContent();

            return Ok(crops);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao listar plantações.",
                    ex.Message
                )
            );
        }
    }
}