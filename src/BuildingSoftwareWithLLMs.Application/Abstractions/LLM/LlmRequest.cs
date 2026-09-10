namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed record LlmRequest
{
    public LlmRequest(
        string model,
        IReadOnlyList<LlmMessage> messages,
        double? temperature = null,
        int? maxOutputTokens = null)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException("Model cannot be empty.", nameof(model));
        }

        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (messages.Count == 0)
        {
            throw new ArgumentException("At least one message is required.", nameof(messages));
        }

        if (temperature is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature),
                temperature,
                "Temperature must be between 0 and 2.");
        }

        if (maxOutputTokens is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxOutputTokens),
                maxOutputTokens,
                "Max output tokens must be greater than zero.");
        }

        Model = model;
        Messages = messages;
        Temperature = temperature;
        MaxOutputTokens = maxOutputTokens;
    }

    public string Model { get; }

    public IReadOnlyList<LlmMessage> Messages { get; }

    public double? Temperature { get; }

    public int? MaxOutputTokens { get; }
}
