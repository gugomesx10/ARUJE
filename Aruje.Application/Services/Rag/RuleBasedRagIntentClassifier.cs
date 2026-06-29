using System.Globalization;
using System.Text;
using Aruje.Application.DTOs.Rag;
using Aruje.Application.Interfaces.Services;

namespace Aruje.Application.Services.Rag;

public class RuleBasedRagIntentClassifier : IRagIntentClassifier
{
    public RagIntentResult Classify(
        string question,
        IReadOnlyList<RagConversationMessageRequest> conversationHistory)
    {
        var normalizedQuestion = Normalize(question);

        if (IsGreeting(normalizedQuestion))
        {
            return new RagIntentResult(
                RagIntentType.Greeting,
                false,
                "Olá! Tudo bem? Eu sou a Arujé IA. Estou aqui para te ajudar a entender sua lavoura de forma simples. Você pode me perguntar sobre alertas, riscos, sensores ou recomendações."
            );
        }

        if (IsHelpRequest(normalizedQuestion))
        {
            return new RagIntentResult(
                RagIntentType.Help,
                false,
                "Claro, eu te ajudo. Pode escrever do seu jeito, mesmo sem usar termos técnicos. Você pode perguntar, por exemplo: “tem algum alerta grave?”, “o que eu faço agora?” ou “explique de forma simples o que aconteceu”."
            );
        }

        if (IsOutOfScope(normalizedQuestion))
        {
            return new RagIntentResult(
                RagIntentType.OutOfScope,
                false,
                "Eu consigo te ajudar melhor com assuntos ligados ao Arujé, como lavoura, sensores, leituras, alertas, riscos e recomendações agrícolas."
            );
        }

        if (IsFollowUpQuestion(normalizedQuestion, conversationHistory))
        {
            return new RagIntentResult(
                RagIntentType.RecommendationQuestion,
                true,
                null
            );
        }

        if (ContainsAny(
                normalizedQuestion,
                "risco",
                "perigo",
                "grave",
                "critico",
                "crítico",
                "problema"))
        {
            return new RagIntentResult(
                RagIntentType.RiskQuestion,
                true,
                null
            );
        }

        if (ContainsAny(
                normalizedQuestion,
                "alerta",
                "alertas",
                "notificacao",
                "notificação",
                "notificacoes",
                "notificações"))
        {
            return new RagIntentResult(
                RagIntentType.AlertQuestion,
                true,
                null
            );
        }

        if (ContainsAny(
                normalizedQuestion,
                "fazer",
                "recomenda",
                "recomendacao",
                "recomendação",
                "acao",
                "ação",
                "agir",
                "resolver"))
        {
            return new RagIntentResult(
                RagIntentType.RecommendationQuestion,
                true,
                null
            );
        }

        if (ContainsAny(
                normalizedQuestion,
                "sensor",
                "sensores",
                "leitura",
                "leituras",
                "temperatura",
                "umidade",
                "solo",
                "luminosidade"))
        {
            return new RagIntentResult(
                RagIntentType.SensorQuestion,
                true,
                null
            );
        }

        return new RagIntentResult(
            RagIntentType.AgricultureQuestion,
            true,
            null
        );
    }

    private static bool IsGreeting(string question)
    {
        return IsExactAny(
                question,
                "oi",
                "ola",
                "olá",
                "eai",
                "e ai")
            || ContainsAny(
                question,
                "bom dia",
                "boa tarde",
                "boa noite",
                "tudo bem");
    }

    private static bool IsHelpRequest(string question)
    {
        return ContainsAny(
            question,
            "preciso de ajuda",
            "pode me ajudar",
            "me ajuda",
            "estou com dificuldade",
            "nao entendi",
            "não entendi",
            "explica melhor",
            "explique melhor",
            "tenho dificuldade"
        );
    }

    private static bool IsOutOfScope(string question)
    {
        return ContainsAny(
            question,
            "futebol",
            "jogo",
            "filme",
            "serie",
            "série",
            "musica",
            "música",
            "receita",
            "carro",
            "viagem",
            "hotel"
        );
    }

    private static bool IsFollowUpQuestion(
        string question,
        IReadOnlyList<RagConversationMessageRequest> conversationHistory)
    {
        if (!conversationHistory.Any())
            return false;

        return ContainsAny(
            question,
            "e agora",
            "o que faco",
            "o que faço",
            "o que eu faco",
            "o que eu faço",
            "como resolvo",
            "como resolver",
            "por que isso",
            "isso e grave",
            "isso é grave",
            "e grave",
            "é grave",
            "e esse alerta",
            "e essa leitura"
        );
    }

    private static bool IsExactAny(
        string text,
        params string[] terms)
    {
        return terms.Any(term => text == Normalize(term));
    }

    private static bool ContainsAny(
        string text,
        params string[] terms)
    {
        return terms.Any(term => text.Contains(Normalize(term)));
    }

    private static string Normalize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var normalized = value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder();

        foreach (var character in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(character);

            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                builder.Append(character);
        }

        return builder
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Trim();
    }
}