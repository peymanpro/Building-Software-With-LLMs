namespace BuildingSoftwareWithLLMs.Infrastructure.LLM;

public sealed class OpenAiCompatibleLlmOptions
{
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";

    public string ApiKey { get; init; } = string.Empty;

    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(60);
}
