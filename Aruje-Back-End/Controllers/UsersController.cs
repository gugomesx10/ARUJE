using Aruje.Application.DTOs.Users;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pelo gerenciamento de usuários.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[SwaggerTag("Gerenciamento de usuários da plataforma Arujé.")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Cadastra um novo usuário.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastrar usuário",
        Description = "Cria um novo usuário na plataforma Arujé com senha protegida por hash."
    )]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> Create(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _userService.CreateAsync(request, cancellationToken);

        return CreatedAtAction(nameof(GetAll), new { id = user.Id }, user);
    }

    /// <summary>
    /// Lista todos os usuários ativos.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Listar usuários",
        Description = "Retorna todos os usuários ativos cadastrados na plataforma."
    )]
    [ProducesResponseType(typeof(IReadOnlyList<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllAsync();

        if (!users.Any())
            return NoContent();

        return Ok(users);
    }
}