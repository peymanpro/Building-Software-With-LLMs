using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.Infrastructure.LLM;

public sealed class InMemoryLlmToolRegistry : ILlmToolRegistry
{
    private readonly IReadOnlyDictionary<string, ILlmTool> _tools;

    public InMemoryLlmToolRegistry(IEnumerable<ILlmTool> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);

        var materialized = tools.ToList();

        var duplicate = materialized
            .GroupBy(tool => tool.Definition.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new ArgumentException(
                $"Duplicate LLM tool name '{duplicate.Key}' was registered.",
                nameof(tools));
        }

        _tools = materialized.ToDictionary(
            tool => tool.Definition.Name,
            StringComparer.OrdinalIgnoreCase);

        Definitions = materialized
            .Select(tool => tool.Definition)
            .ToArray();
    }

    public IReadOnlyList<LlmToolDefinition> Definitions { get; }

    public bool TryResolve(string name, out ILlmTool tool)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _tools.TryGetValue(name, out tool!);
    }
}
