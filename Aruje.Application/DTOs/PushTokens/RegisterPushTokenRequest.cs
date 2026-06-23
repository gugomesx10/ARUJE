using System.ComponentModel.DataAnnotations;

namespace Aruje.Application.DTOs.PushTokens;

public class RegisterPushTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    public string Platform { get; set; } = string.Empty;
}