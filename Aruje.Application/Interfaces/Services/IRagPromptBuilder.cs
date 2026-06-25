using Aruje.Application.DTOs.Rag;

namespace Aruje.Application.Interfaces.Services;

public interface IRagPromptBuilder
{
    string BuildPrompt(RagContext context);
}