namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmResponse
{
    public LlmResponse(
        string model,
        string? content,
        LlmFinishReason finishReason,
        LlmUsage? usage = null,
        IReadOnlyList<LlmToolCall>? toolCalls = null)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model cannot be empty.", nameof(model));
        }

        if (string.IsNullOrWhiteSpace(content) &&
            (toolCalls is null || toolCalls.Count == 0))
        {
            throw new ArgumentException(
                "Response must contain content or at least one tool call.",
                nameof(content));
        }

        Model = model;
        Content = content;
        FinishReason = finishReason;
        Usage = usage;
        ToolCalls = toolCalls ?? [];
    }

    public string Model { get; }

    public string? Content { get; }

    public LlmFinishReason FinishReason { get; }

    public LlmUsage? Usage { get; }

    public IReadOnlyList<LlmToolCall> ToolCalls { get; }

    public bool HasToolCalls => ToolCalls.Count > 0;
}
