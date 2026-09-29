using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Application.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class LlmChatServiceTests
{
    [Fact]
    public async Task ExecuteAsync_ExecutesToolAndContinuesConversation()
    {
        var registry = new InMemoryLlmToolRegistry([new GetOrderStatusTool()]);
        var service = new LlmChatService(
            new FakeLlmProvider(),
            registry);

        var result = await service.ExecuteAsync(
            [new LlmMessage(LlmRole.User, "Where is order ORD-1002?")],
            "fake-model");

        Assert.Equal(2, result.ProviderCalls);
        Assert.Equal(1, result.ToolCallsExecuted);
        Assert.Contains("ORD-1002", result.Response.Content);
        Assert.Contains("Delayed", result.Response.Content);
        Assert.Equal(4, result.Messages.Count);
        Assert.Equal(LlmRole.Assistant, result.Messages[1].Role);
        Assert.True(result.Messages[1].HasToolCalls);
        Assert.Equal(LlmRole.Tool, result.Messages[2].Role);
        Assert.Equal(LlmRole.Assistant, result.Messages[3].Role);
    }

    [Fact]
    public async Task ExecuteAsync_GeneralQuestion_DoesNotExecuteTools()
    {
        var registry = new InMemoryLlmToolRegistry([new GetOrderStatusTool()]);
        var service = new LlmChatService(
            new FakeLlmProvider(),
            registry);

        var result = await service.ExecuteAsync(
            [new LlmMessage(LlmRole.User, "Hello there.")],
            "fake-model");

        Assert.Equal(1, result.ProviderCalls);
        Assert.Equal(0, result.ToolCallsExecuted);
        Assert.Contains("general_inquiry", result.Response.Content);
        Assert.Equal(2, result.Messages.Count);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownTool_IsReported()
    {
        var registry = new InMemoryLlmToolRegistry([new GetOrderStatusTool()]);
        var provider = new StubProvider(
            new LlmResponse(
                "fake-model",
                null,
                LlmFinishReason.ToolCall,
                toolCalls:
                [
                    new LlmToolCall("call-1", "missing_tool", [])
                ]));

        var service = new LlmChatService(provider, registry);

        var exception = await Assert.ThrowsAsync<LlmToolExecutionException>(() =>
            service.ExecuteAsync(
                [new LlmMessage(LlmRole.User, "Use the missing tool.")],
                "fake-model"));

        Assert.Contains("missing_tool", exception.Message);
    }

    private sealed class StubProvider : ILLMProvider
    {
        private readonly LlmResponse _response;

        public StubProvider(LlmResponse response)
        {
            _response = response;
        }

        public Task<LlmResponse> CompleteAsync(
            LlmRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_response);
    }
}
