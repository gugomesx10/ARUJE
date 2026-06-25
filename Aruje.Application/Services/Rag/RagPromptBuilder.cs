using System.Text;
using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;

namespace Aruje.Application.Services.Rag;

public class RagPromptBuilder : IRagPromptBuilder
{
    public string BuildPrompt(RagContext context)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Você é o Assistente Inteligente do Arujé.");
        builder.AppendLine("Sua função é responder perguntas sobre monitoramento agrícola usando apenas os dados recuperados do sistema.");
        builder.AppendLine();
        builder.AppendLine("Regras:");
        builder.AppendLine("- Responda em português do Brasil.");
        builder.AppendLine("- Seja claro, técnico e objetivo.");
        builder.AppendLine("- Use somente o contexto fornecido.");
        builder.AppendLine("- Se não houver dados suficientes, diga que os dados são insuficientes.");
        builder.AppendLine("- Sempre explique o motivo da resposta.");
        builder.AppendLine("- Sempre gere uma recomendação prática.");
        builder.AppendLine("- Não invente sensores, leituras, alertas ou análises que não estejam no contexto.");
        builder.AppendLine();
        builder.AppendLine($"Pergunta do usuário: {context.Question}");
        builder.AppendLine();
        builder.AppendLine("Contexto recuperado do banco de dados:");

        if (!context.Items.Any())
        {
            builder.AppendLine("- Nenhum contexto encontrado.");
            return builder.ToString();
        }

        foreach (var item in context.Items)
        {
            builder.AppendLine();
            builder.AppendLine($"Fonte: {item.Type}");
            builder.AppendLine($"Id: {item.Id}");
            builder.AppendLine($"Título: {item.Title}");
            builder.AppendLine($"Relevância: {item.RelevanceScore}");
            builder.AppendLine($"Criado em: {item.CreatedAt:dd/MM/yyyy HH:mm}");
            builder.AppendLine($"Conteúdo: {item.Content}");
        }

        builder.AppendLine();
        builder.AppendLine("Com base no contexto acima, responda no seguinte formato:");
        builder.AppendLine("Resposta:");
        builder.AppendLine("Nível de risco:");
        builder.AppendLine("Recomendação:");
        builder.AppendLine("Fontes consideradas:");

        return builder.ToString();
    }
}