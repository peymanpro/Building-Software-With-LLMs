using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class FakeLlmProviderTests
{
    [Fact]
    public async Task CompleteAsync_WithValidRequest_ReturnsDeterministicResponse()
    {
        var provider = new FakeLlmProvider();

        var request = new LlmRequest(
            model: "fake-model",
            messages:
            [
                new LlmMessage(
                    LlmRole.User,
                    "My order is late.")
            ]);

        var response = await provider.CompleteAsync(request);

        Assert.Equal("fake-model", response.Model);
        Assert.Equal(
            """{"intent":"general_inquiry","confidence":0.90}""",
            response.Content);
        Assert.Equal(LlmFinishReason.Stop, response.FinishReason);
        Assert.False(response.HasToolCalls);
        Assert.Null(response.Usage);
    }

    [Fact]
    public async Task CompleteAsync_WithCancellationToken_CanBeCancelled()
    {
        var provider = new FakeLlmProvider();

        var request = new LlmRequest(
            model: "fake-model",
            messages:
            [
                new LlmMessage(
                    LlmRole.User,
                    "My order is late.")
            ]);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            provider.CompleteAsync(
                request,
                cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CompleteAsync_WithNullRequest_Throws()
    {
        var provider = new FakeLlmProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.CompleteAsync(null!));
    }

    [Fact]
    public async Task CompleteAsync_WithSameRequest_IsDeterministic()
    {
        var provider = new FakeLlmProvider();

        var request = new LlmRequest(
            model: "fake-model",
            messages:
            [
                new LlmMessage(
                    LlmRole.User,
                    "Where is my order?")
            ]);

        var first = await provider.CompleteAsync(request);
        var second = await provider.CompleteAsync(request);

        Assert.Equal(first.Model, second.Model);
        Assert.Equal(first.Content, second.Content);
        Assert.Equal(first.FinishReason, second.FinishReason);
        Assert.Equal(first.ToolCalls.Count, second.ToolCalls.Count);
    }
}
