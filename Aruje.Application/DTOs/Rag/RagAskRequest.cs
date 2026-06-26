using System.ComponentModel.DataAnnotations;

namespace Aruje.Application.DTOs.Rag;

public class RagAskRequest
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MinLength(2, ErrorMessage = "A pergunta deve ter pelo menos 2 caracteres.")]
    [MaxLength(800, ErrorMessage = "A pergunta deve ter no máximo 800 caracteres.")]
    public string Question { get; set; } = string.Empty;

    public int MaxItems { get; set; } = 8;

    public IReadOnlyList<RagConversationMessageRequest> ConversationHistory { get; set; } =
        Array.Empty<RagConversationMessageRequest>();
}