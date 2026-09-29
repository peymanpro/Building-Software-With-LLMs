namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public sealed class LlmToolExecutionException : Exception
{
    public LlmToolExecutionException(string message)
        : base(message)
    {
    }

    public LlmToolExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
