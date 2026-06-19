using Aruje.Application.DTOs.Sensors;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
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
    /// Lista todos os sensores ativos.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar sensores",
        Description = "Retorna todos os sensores IoT ativos cadastrados no sistema."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<SensorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SensorResponse>>> GetAll()
    {
        var sensors = await _sensorService.GetAllAsync();

        if (!sensors.Any())
            return NoContent();

        return Ok(sensors);
    }

    /// <summary>
    /// Busca um sensor pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar sensor por ID",
        Description = "Retorna os dados de um sensor específico."
    )]
    [ProducesResponseType(typeof(SensorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SensorResponse>> GetById([FromRoute] Guid id)
    {
        var sensor = await _sensorService.GetByIdAsync(id);

        return Ok(sensor);
    }

    /// <summary>
    /// Lista sensores vinculados a uma plantação.
    /// </summary>
    [HttpGet("by-crop/{cropId:guid}")]
    [SwaggerOperation(
        Summary = "Listar sensores por plantação",
        Description = "Retorna todos os sensores vinculados a uma plantação específica."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<SensorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<SensorResponse>>> GetByCropId(
        [FromRoute] Guid cropId)
    {
        var sensors = await _sensorService.GetByCropIdAsync(cropId);

        if (!sensors.Any())
            return NoContent();

        return Ok(sensors);
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
        var sensor = await _sensorService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = sensor.Id },
            sensor
        );
    }

    /// <summary>
    /// Atualiza um sensor existente.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualizar sensor",
        Description = "Atualiza os dados principais de um sensor IoT existente."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSensorRequest request,
        CancellationToken cancellationToken)
    {
        await _sensorService.UpdateAsync(id, request, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Remove logicamente um sensor.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover sensor",
        Description = "Realiza a remoção lógica de um sensor, mantendo o histórico no banco."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _sensorService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}