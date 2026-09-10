namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmResponse
{
    public LlmResponse(
        string model,
        string content,
        LlmFinishReason finishReason,
        LlmUsage? usage = null)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model cannot be empty.", nameof(model));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Response content cannot be empty.", nameof(content));
        }

        Model = model;
        Content = content;
        FinishReason = finishReason;
        Usage = usage;
    }

    public string Model { get; }

    public string Content { get; }

    public LlmFinishReason FinishReason { get; }

    public LlmUsage? Usage { get; }
}
