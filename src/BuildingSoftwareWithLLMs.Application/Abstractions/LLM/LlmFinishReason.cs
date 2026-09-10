namespace BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

public enum LlmFinishReason
{
    Stop,
    Length,
    ToolCall,
    ContentFilter,
    Unknown
}
