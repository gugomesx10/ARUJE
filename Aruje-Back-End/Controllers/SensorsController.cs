using Aruje.Application.DTOs.Sensors;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo gerenciamento de sensores IoT.
/// </summary>
[ApiController]
[Route("api/sensors")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de sensores IoT vinculados às plantações.")]
public class SensorsController : ControllerBase
{
    private readonly SensorService _sensorService;

    public SensorsController(SensorService sensorService)
    {
        _sensorService = sensorService;
    }

    /// <summary>
    /// Cadastra um novo sensor IoT.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastrar sensor",
        Description = "Cria um novo sensor IoT vinculado a uma plantação existente."
    )]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SensorResponse>> Create(
        [FromBody] CreateSensorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var sensor = await _sensorService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(GetByCropId), new { cropId = sensor.CropId }, sensor);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Crop not found"))
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
                "Conflito ao cadastrar sensor. Verifique se o número serial já existe.",
                ex.InnerException?.Message
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao cadastrar sensor.",
                    ex.Message
                )
            );
        }
    }

    /// <summary>
    /// Lista sensores de uma plantação.
    /// </summary>
    [HttpGet("by-crop/{cropId:guid}")]
    [SwaggerOperation(
        Summary = "Listar sensores por plantação",
        Description = "Retorna todos os sensores ativos vinculados a uma plantação específica."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<SensorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SensorResponse>>> GetByCropId([FromRoute] Guid cropId)
    {
        try
        {
            var sensors = await _sensorService.GetByCropIdAsync(cropId);

            if (!sensors.Any())
                return NoContent();

            return Ok(sensors);
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao listar sensores.",
                    ex.Message
                )
            );
        }
    }
}