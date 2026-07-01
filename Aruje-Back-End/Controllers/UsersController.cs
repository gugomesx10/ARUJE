using Aruje.Application.DTOs.Users;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Authorization;
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
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
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

    /// <summary>
    /// Busca um usuário pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Buscar usuário por ID",
        Description = "Retorna os dados de um usuário específico."
    )]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetById([FromRoute] Guid id)
    {
        var user = await _userService.GetByIdAsync(id);

        return Ok(user);
    }

    /// <summary>
    /// Busca um usuário pelo email.
    /// </summary>
    [HttpGet("by-email")]
    [SwaggerOperation(
        Summary = "Buscar usuário por email",
        Description = "Retorna os dados de um usuário específico a partir do email informado."
    )]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserResponse>> GetByEmail([FromQuery] string email)
    {
        var user = await _userService.GetByEmailAsync(email);

        return Ok(user);
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
        var user = await _userService.CreateAsync(
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user
        );
    }

    /// <summary>
    /// Atualiza os dados principais de um usuário.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(
        Summary = "Atualizar usuário",
        Description = "Atualiza nome completo e email de um usuário existente."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        await _userService.UpdateAsync(
            id,
            request,
            cancellationToken
        );

        return NoContent();
    }

    /// <summary>
    /// Altera o perfil de acesso de um usuário.
    /// </summary>
    [HttpPatch("{id:guid}/change-role")]
    [SwaggerOperation(
        Summary = "Alterar perfil do usuário",
        Description = "Altera o papel de acesso de um usuário, como Admin, Manager ou Operator."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangeRole(
        [FromRoute] Guid id,
        [FromBody] ChangeUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        await _userService.ChangeRoleAsync(
            id,
            request,
            cancellationToken
        );

        return NoContent();
    }

    /// <summary>
    /// Altera a senha de um usuário.
    /// </summary>
    [HttpPatch("{id:guid}/change-password")]
    [SwaggerOperation(
        Summary = "Alterar senha do usuário",
        Description = "Altera a senha de um usuário e armazena apenas o hash no banco."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ChangePassword(
        [FromRoute] Guid id,
        [FromBody] ChangeUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _userService.ChangePasswordAsync(
            id,
            request,
            cancellationToken
        );

        return NoContent();
    }

    /// <summary>
    /// Remove logicamente um usuário.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(
        Summary = "Remover usuário",
        Description = "Realiza a remoção lógica de um usuário, mantendo o histórico no banco."
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _userService.DeleteAsync(
            id,
            cancellationToken
        );

        return NoContent();
    }
}