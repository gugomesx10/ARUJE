using System.ComponentModel.DataAnnotations;

namespace Aruje.Application.DTOs.Rag;

public class RagConversationMessageRequest
{
    [Required(ErrorMessage = "O papel da mensagem é obrigatório.")]
    [MaxLength(30, ErrorMessage = "O papel da mensagem deve ter no máximo 30 caracteres.")]
    public String Role { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "O conteúdo da mensagem é obrigatório.")]
    [MaxLength(1200,  ErrorMessage = "O conteúdo da mensagem deve ter no máximo 1200 caracteres.")]
    public string Content { get; set; } = string.Empty;

}