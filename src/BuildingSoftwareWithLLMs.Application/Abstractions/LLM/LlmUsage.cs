namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmUsage
{
    public LlmUsage(int inputTokens, int outputTokens)
    {
        if (inputTokens < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(inputTokens),
                inputTokens,
                "Input token count cannot be negative.");
        }

        if (outputTokens < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(outputTokens),
                outputTokens,
                "Output token count cannot be negative.");
        }

        InputTokens = inputTokens;
        OutputTokens = outputTokens;
    }

    public int InputTokens { get; }

    public int OutputTokens { get; }

    public int TotalTokens => InputTokens + OutputTokens;
}
