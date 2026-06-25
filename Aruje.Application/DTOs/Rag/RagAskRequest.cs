using System.ComponentModel.DataAnnotations;

namespace Aruje.Application.DTOs.Rag;

public class RagAskRequest
{
    [Required(ErrorMessage = "A pergunta é obrigatória.")]
    [MinLength(5, ErrorMessage = "A pergunta deve ter pelo menos 5 caracteres.")]
    [MaxLength(800, ErrorMessage = "A pergunta deve ter no máximo 800 caracteres.")]
    public string Question { get; set; } = string.Empty;

    public int MaxItems { get; set; } = 8;
}