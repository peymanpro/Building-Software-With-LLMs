using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class ILLMProviderTests
{
    [Fact]
    public async Task CompleteAsync_ProviderCanImplementContract()
    {
        var provider = new TestLlmProvider();

        var request = new LlmRequest(
            model: "test-model",
            messages:
            [
                new LlmMessage(
                    LlmRole.User,
                    "Classify this support ticket.")
            ]);

        var response = await provider.CompleteAsync(request);

        Assert.Equal("test-model", response.Model);
        Assert.Equal("classified", response.Content);
        Assert.Equal(LlmFinishReason.Stop, response.FinishReason);
        Assert.Same(request, provider.LastRequest);
    }

    [Fact]
    public async Task CompleteAsync_PassesCancellationTokenToImplementation()
    {
        var provider = new TestLlmProvider();
        using var cancellationTokenSource = new CancellationTokenSource();

        var request = new LlmRequest(
            model: "test-model",
            messages:
            [
                new LlmMessage(
                    LlmRole.User,
                    "Analyze ticket.")
            ]);

        await provider.CompleteAsync(
            request,
            cancellationTokenSource.Token);

        Assert.Equal(
            cancellationTokenSource.Token,
            provider.LastCancellationToken);
    }

    private sealed class TestLlmProvider : ILLMProvider
    {
        public LlmRequest? LastRequest { get; private set; }

        public CancellationToken LastCancellationToken { get; private set; }

        public Task<LlmResponse> CompleteAsync(
            LlmRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            LastCancellationToken = cancellationToken;

            var response = new LlmResponse(
                model: request.Model,
                content: "classified",
                finishReason: LlmFinishReason.Stop);

            return Task.FromResult(response);
        }
    }
}
