using Aruje.Application.DTOs.SensorReadings;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo recebimento e consulta de leituras IoT.
/// </summary>
[ApiController]
[Route("api/sensor-readings")]
[Produces("application/json")]
[SwaggerTag("Recebimento e processamento de leituras IoT enviadas por sensores.")]
[Authorize(Roles = "Admin,Manager,Operator")]
public class SensorReadingsController : ControllerBase
{
    private readonly SensorReadingService _sensorReadingService;

    public SensorReadingsController(SensorReadingService sensorReadingService)
    {
        _sensorReadingService = sensorReadingService;
    }

    /// <summary>
    /// Lista todas as leituras ativas.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar leituras",
        Description = "Retorna todas as leituras IoT registradas no sistema."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<SensorReadingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetAll()
    {
        var readings = await _sensorReadingService.GetAllAsync();

        if (!readings.Any())
            return NoContent();

        return Ok(readings);
    }

    /// <summary>
    /// Busca uma leitura pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar leitura por ID",
        Description = "Retorna os dados de uma leitura IoT específica."
    )]
    [ProducesResponseType(typeof(SensorReadingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SensorReadingResponse>> GetById([FromRoute] Guid id)
    {
        var reading = await _sensorReadingService.GetByIdAsync(id);

        return Ok(reading);
    }

    /// <summary>
    /// Lista leituras vinculadas a um sensor.
    /// </summary>
    [HttpGet("by-sensor/{sensorId:guid}")]
    [SwaggerOperation(
        Summary = "Listar leituras por sensor",
        Description = "Retorna todas as leituras registradas por um sensor específico."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<SensorReadingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetBySensorId(
        [FromRoute] Guid sensorId)
    {
        var readings = await _sensorReadingService.GetBySensorIdAsync(sensorId);

        if (!readings.Any())
            return NoContent();

        return Ok(readings);
    }

    /// <summary>
    /// Lista as últimas leituras de um sensor.
    /// </summary>
    [HttpGet("latest/{sensorId:guid}")]
    [SwaggerOperation(
        Summary = "Listar últimas leituras",
        Description = "Retorna as últimas leituras registradas por um sensor específico, limitadas pela quantidade informada."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<SensorReadingResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SensorReadingResponse>>> GetLatestBySensorId(
        [FromRoute] Guid sensorId,
        [FromQuery] int quantity = 10)
    {
        var readings = await _sensorReadingService.GetLatestBySensorIdAsync(
            sensorId,
            quantity
        );

        if (!readings.Any())
            return NoContent();

        return Ok(readings);
    }

    /// <summary>
    /// Registra uma nova leitura IoT.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Registrar leitura IoT",
        Description = "Recebe dados de sensores IoT, registra a leitura e dispara o fluxo automático de geração de alerta e análise inteligente quando necessário."
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
        var reading = await _sensorReadingService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = reading.Id },
            reading
        );
    }

    /// <summary>
    /// Remove logicamente uma leitura.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover leitura",
        Description = "Realiza a remoção lógica de uma leitura IoT, mantendo o histórico no banco."
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
        await _sensorReadingService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}