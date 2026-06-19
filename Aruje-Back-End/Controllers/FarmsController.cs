using Aruje.Application.DTOs.Farms;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo gerenciamento de fazendas.
/// </summary>
[ApiController]
[Route("api/farms")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de fazendas monitoradas pelo Arujé.")]
[Authorize(Roles = "Admin,Manager")]
public class FarmsController : ControllerBase
{
    private readonly FarmService _farmService;

    public FarmsController(FarmService farmService)
    {
        _farmService = farmService;
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
        var farms = await _farmService.GetAllAsync();

        if (!farms.Any())
            return NoContent();

        return Ok(farms);
    }

    /// <summary>
    /// Busca uma fazenda pelo identificador.
    /// </summary>
    /// <param name="id">Identificador da fazenda.</param>
    /// <returns>Dados da fazenda encontrada.</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar fazenda por ID",
        Description = "Retorna os dados de uma fazenda específica a partir do seu identificador."
    )]
    [ProducesResponseType(typeof(FarmResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FarmResponse>> GetById([FromRoute] Guid id)
    {
        var farm = await _farmService.GetByIdAsync(id);

        return Ok(farm);
    }

    /// <summary>
    /// Pesquisa fazendas pelo nome.
    /// </summary>
    /// <param name="name">Nome ou parte do nome da fazenda.</param>
    /// <returns>Lista de fazendas encontradas.</returns>
    [HttpGet("search")]
    [SwaggerOperation(
        Summary = "Pesquisar fazendas por nome",
        Description = "Retorna fazendas cujo nome contenha o texto informado."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<FarmResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<FarmResponse>>> SearchByName(
        [FromQuery] string name)
    {
        var farms = await _farmService.SearchByNameAsync(name);

        if (!farms.Any())
            return NoContent();

        return Ok(farms);
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
        var farm = await _farmService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = farm.Id },
            farm
        );
    }

    /// <summary>
    /// Atualiza os dados de uma fazenda existente.
    /// </summary>
    /// <param name="id">Identificador da fazenda.</param>
    /// <param name="request">Novos dados da fazenda.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualizar fazenda",
        Description = "Atualiza os dados principais de uma fazenda existente."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateFarmRequest request,
        CancellationToken cancellationToken)
    {
        await _farmService.UpdateAsync(id, request, cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Remove logicamente uma fazenda.
    /// </summary>
    /// <param name="id">Identificador da fazenda.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover fazenda",
        Description = "Realiza a remoção lógica de uma fazenda, mantendo o histórico no banco de dados."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _farmService.DeleteAsync(id, cancellationToken);

        return NoContent();
    }
}