using Aruje.Application.DTOs.PushTokens;
using Aruje.Domain.Entities;
using Aruje.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Aruje.API.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento dos tokens de push notification dos dispositivos.
/// </summary>
[ApiController]
[Route("push-tokens")]
[Authorize]
public class PushTokensController : ControllerBase
{
    private readonly ArujeDbContext _context;

    public PushTokensController(ArujeDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Registra ou atualiza o token de push notification do dispositivo autenticado.
    /// </summary>
    /// <param name="request">Dados do token gerado pelo aplicativo mobile.</param>
    /// <param name="cancellationToken">Token de cancelamento da requisição.</param>
    /// <returns>Retorna o token registrado ou atualizado.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Registra ou atualiza um token de push notification",
        Description = "Recebe o token gerado pelo aplicativo mobile via Expo Notifications e salva no banco de dados para permitir o envio de notificações push."
    )]
    [SwaggerResponse(StatusCodes.Status201Created, "Token registrado com sucesso.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Token atualizado com sucesso.")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Token inválido ou não informado.")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Usuário não autenticado.")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterPushTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return BadRequest(new
            {
                message = "Token é obrigatório."
            });
        }

        var existingToken = await _context.PushTokens
            .FirstOrDefaultAsync(x => x.Token == request.Token, cancellationToken);

        if (existingToken is not null)
        {
            existingToken.Platform = request.Platform;
            existingToken.IsActive = true;
            existingToken.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                message = "Push token atualizado com sucesso.",
                token = existingToken.Token
            });
        }

        var pushToken = new PushToken
        {
            Token = request.Token,
            Platform = request.Platform,
            IsActive = true
        };

        _context.PushTokens.Add(pushToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Created(string.Empty, new
        {
            message = "Push token registrado com sucesso.",
            token = pushToken.Token
        });
    }
}