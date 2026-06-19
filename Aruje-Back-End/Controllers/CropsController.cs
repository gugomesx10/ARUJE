using Aruje.Application.DTOs.Crops;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
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
    /// Lista todas as plantações ativas.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar plantações",
        Description = "Retorna todas as plantações ativas cadastradas no sistema."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<CropResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<CropResponse>>> GetAll()
    {
        var crops = await _cropService.GetAllAsync();

        if (!crops.Any())
            return NoContent();

        return Ok(crops);
    }

    /// <summary>
    /// Busca uma plantação pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar plantação por ID",
        Description = "Retorna os dados de uma plantação específica."
    )]
    [ProducesResponseType(typeof(CropResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CropResponse>> GetById([FromRoute] Guid id)
    {
        var crop = await _cropService.GetByIdAsync(id);

        return Ok(crop);
    }

    /// <summary>
    /// Lista plantações por fazenda.
    /// </summary>
    [HttpGet("by-farm/{farmId:guid}")]
    [SwaggerOperation(
        Summary = "Listar plantações por fazenda",
        Description = "Retorna todas as plantações vinculadas a uma fazenda específica."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<CropResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<CropResponse>>> GetByFarmId(
        [FromRoute] Guid farmId)
    {
        var crops = await _cropService.GetByFarmIdAsync(farmId);

        if (!crops.Any())
            return NoContent();

        return Ok(crops);
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
        var crop = await _cropService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = crop.Id },
            crop
        );
    }

    /// <summary>
    /// Atualiza uma plantação existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualizar plantação",
        Description = "Atualiza os dados principais de uma plantação existente."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateCropRequest request,
        CancellationToken cancellationToken)
    {
        await _cropService.UpdateAsync(id, request, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Remove logicamente uma plantação.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover plantação",
        Description = "Realiza a remoção lógica da plantação, mantendo o histórico no banco."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _cropService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}