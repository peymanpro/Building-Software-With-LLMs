namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmMessage
{
    public LlmMessage(LlmRole role, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Message content cannot be empty.", nameof(content));
        }

        Role = role;
        Content = content;
    }

    public LlmRole Role { get; }

    public string Content { get; }
}
