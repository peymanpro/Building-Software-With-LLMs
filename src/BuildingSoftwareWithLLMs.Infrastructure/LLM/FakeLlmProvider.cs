using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.Infrastructure.LLM;

public sealed class FakeLlmProvider : ILLMProvider
{
    public Task<LlmResponse> CompleteAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        cancellationToken.ThrowIfCancellationRequested();

        var response = new LlmResponse(
            model: request.Model,
            content: """{"intent":"general_inquiry","confidence":0.90}""",
            finishReason: LlmFinishReason.Stop);

        return Task.FromResult(response);
    }
}
