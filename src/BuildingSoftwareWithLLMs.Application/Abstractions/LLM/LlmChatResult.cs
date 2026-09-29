namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmChatResult(
    LlmResponse Response,
    IReadOnlyList<LlmMessage> Messages,
    int ProviderCalls,
    int ToolCallsExecuted);
