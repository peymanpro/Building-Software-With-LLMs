namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public interface ILlmToolRegistry
{
    IReadOnlyList<LlmToolDefinition> Definitions { get; }

    bool TryResolve(string name, out ILlmTool tool);
}
