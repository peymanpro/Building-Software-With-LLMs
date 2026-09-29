using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class FakeLlmProviderTests
{
    [Fact]
    public async Task CompleteAsync_GeneralQuestion_ReturnsDeterministicJson()
    {
        var provider = new FakeLlmProvider();

        var request = new LlmRequest(
            "fake-model",
            [new LlmMessage(LlmRole.User, "Hello there.")]);

        var response = await provider.CompleteAsync(request);

        Assert.Equal("fake-model", response.Model);
        Assert.Equal(
            """{"intent":"general_inquiry","confidence":0.90}""",
            response.Content);
        Assert.Equal(LlmFinishReason.Stop, response.FinishReason);
        Assert.False(response.HasToolCalls);
    }

    [Fact]
    public async Task CompleteAsync_OrderQuestion_ReturnsGetOrderStatusToolCall()
    {
        var provider = new FakeLlmProvider();

        var request = new LlmRequest(
            "fake-model",
            [new LlmMessage(LlmRole.User, "Where is order ORD-1002?")],
            tools:
            [
                new LlmToolDefinition(
                    "get_order_status",
                    "Returns order status.",
                    """{"type":"object","properties":{"orderId":{"type":"string"}},"required":["orderId"]}""")
            ]);

        var response = await provider.CompleteAsync(request);

        Assert.True(response.HasToolCalls);
        Assert.Equal(LlmFinishReason.ToolCall, response.FinishReason);
        var toolCall = Assert.Single(response.ToolCalls);
        Assert.Equal("get_order_status", toolCall.Name);
        var argument = Assert.Single(toolCall.Arguments);
        Assert.Equal("orderId", argument.Name);
        Assert.Equal("ORD-1002", argument.Value);
    }

    [Fact]
    public async Task CompleteAsync_WithCancellationToken_CanBeCancelled()
    {
        var provider = new FakeLlmProvider();

        var request = new LlmRequest(
            "fake-model",
            [new LlmMessage(LlmRole.User, "Hello.")]);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            provider.CompleteAsync(request, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task CompleteAsync_WithNullRequest_Throws()
    {
        var provider = new FakeLlmProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.CompleteAsync(null!));
    }
}
