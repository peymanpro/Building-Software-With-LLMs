namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmToolCallArgument
{
    public LlmToolCallArgument(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Argument name cannot be empty.", nameof(name));
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        Name = name;
        Value = value;
    }

    public string Name { get; }

    public string Value { get; }
}
