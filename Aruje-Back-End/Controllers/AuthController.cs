using Aruje.Application.DTOs.Auth;
using Aruje.Application.Services;
using Aruje_Back_End.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje_Back_End.Controllers;

/// <summary>
/// Endpoints responsáveis pela autenticação de usuários.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[SwaggerTag("Autenticação e geração de token JWT.")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza login do usuário.
    /// </summary>
    /// <param name="request">Credenciais de acesso do usuário.</param>
    /// <returns>Token JWT e dados básicos do usuário autenticado.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Realizar login",
        Description = "Autentica o usuário com email e senha e retorna um token JWT válido."
    )]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        return Ok(response);
    }
}