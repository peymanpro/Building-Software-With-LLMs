namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public interface ILlmChatService
{
    Task<LlmChatResult> ExecuteAsync(
        IReadOnlyList<LlmMessage> messages,
        string model,
        double? temperature = null,
        int? maxOutputTokens = null,
        CancellationToken cancellationToken = default);
}
