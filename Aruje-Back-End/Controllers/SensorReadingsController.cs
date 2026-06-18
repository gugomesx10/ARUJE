using Aruje.Application.DTOs.SensorReadings;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo recebimento de leituras dos sensores IoT.
/// </summary>
[ApiController]
[Route("api/sensor-readings")]
[Produces("application/json")]
[SwaggerTag("Recebimento e processamento de leituras IoT enviadas por sensores.")]
public class SensorReadingsController : ControllerBase
{
    private readonly SensorReadingService _sensorReadingService;

    public SensorReadingsController(SensorReadingService sensorReadingService)
    {
        _sensorReadingService = sensorReadingService;
    }

    /// <summary>
    /// Registra uma nova leitura de sensor IoT.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Registrar leitura IoT",
        Description = "Recebe dados de sensores IoT, registra a leitura e dispara o fluxo de geração de alerta e análise inteligente quando necessário."
    )]
    [ProducesResponseType(typeof(SensorReadingResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SensorReadingResponse>> Create(
        [FromBody] CreateSensorReadingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var reading = await _sensorReadingService.CreateAsync(request, cancellationToken);

            return CreatedAtAction(nameof(Create), new { id = reading.Id }, reading);
        }
        catch (ArgumentException ex) when (ex.Message.Contains("Sensor not found"))
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
                "Conflito ao registrar leitura do sensor.",
                ex.InnerException?.Message
            ));
        }
        catch (Exception ex)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiErrorResponse(
                    StatusCodes.Status500InternalServerError,
                    "Erro inesperado ao registrar leitura do sensor.",
                    ex.Message
                )
            );
        }
    }
}