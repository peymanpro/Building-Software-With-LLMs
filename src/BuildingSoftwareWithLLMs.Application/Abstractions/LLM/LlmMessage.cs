namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmMessage
{
    public LlmMessage(
        LlmRole role,
        string? content,
        IReadOnlyList<LlmToolCall>? toolCalls = null,
        string? toolCallId = null,
        string? name = null)
    {
        if (string.IsNullOrWhiteSpace(content) &&
            (toolCalls is null || toolCalls.Count == 0))
        {
            throw new ArgumentException(
                "Message must contain content or at least one tool call.",
                nameof(content));
        }

        if (role == LlmRole.Tool && string.IsNullOrWhiteSpace(toolCallId))
        {
            throw new ArgumentException(
                "Tool messages require a tool call id.",
                nameof(toolCallId));
        }

        Role = role;
        Content = content;
        ToolCalls = toolCalls ?? [];
        ToolCallId = toolCallId;
        Name = name;
    }

    public LlmRole Role { get; }

    public string? Content { get; }

    public IReadOnlyList<LlmToolCall> ToolCalls { get; }

    public string? ToolCallId { get; }

    public string? Name { get; }

    public bool HasToolCalls => ToolCalls.Count > 0;
}
