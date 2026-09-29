using System.Text.Json;

namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmToolDefinition
{
    public LlmToolDefinition(
        string name,
        string description,
        string parametersJsonSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Tool name cannot be empty.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Tool description cannot be empty.",
                nameof(description));
        }

        if (string.IsNullOrWhiteSpace(parametersJsonSchema))
        {
            throw new ArgumentException(
                "Tool parameter schema cannot be empty.",
                nameof(parametersJsonSchema));
        }

        try
        {
            using var document = JsonDocument.Parse(parametersJsonSchema);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException(
                    "Tool parameter schema must be a JSON object.",
                    nameof(parametersJsonSchema));
            }
        }
        catch (JsonException exception)
        {
            throw new ArgumentException(
                "Tool parameter schema must contain valid JSON.",
                nameof(parametersJsonSchema),
                exception);
        }

        Name = name;
        Description = description;
        ParametersJsonSchema = parametersJsonSchema;
    }

    public string Name { get; }

    public string Description { get; }

    public string ParametersJsonSchema { get; }
}
