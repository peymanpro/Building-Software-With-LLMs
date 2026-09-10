namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmToolCall
{
    public LlmToolCall(
        string id,
        string name,
        IReadOnlyList<LlmToolCallArgument> arguments)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Tool call id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tool name cannot be empty.", nameof(name));
        }

        if (arguments is null)
        {
            throw new ArgumentNullException(nameof(arguments));
        }

        Id = id;
        Name = name;
        Arguments = arguments;
    }

    public string Id { get; }

    public string Name { get; }

    public IReadOnlyList<LlmToolCallArgument> Arguments { get; }
}
