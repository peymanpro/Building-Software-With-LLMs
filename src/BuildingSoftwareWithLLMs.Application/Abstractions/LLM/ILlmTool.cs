namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public interface ILlmTool
{
    LlmToolDefinition Definition { get; }

    Task<string> ExecuteAsync(
        IReadOnlyList<LlmToolCallArgument> arguments,
        CancellationToken cancellationToken = default);
}
