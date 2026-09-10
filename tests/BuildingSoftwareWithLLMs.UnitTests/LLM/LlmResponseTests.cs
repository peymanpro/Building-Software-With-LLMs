using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class LlmResponseTests
{
    [Fact]
    public void Constructor_WithValidTextResponse_CreatesResponse()
    {
        var usage = new LlmUsage(
            inputTokens: 120,
            outputTokens: 45);

        var response = new LlmResponse(
            model: "test-model",
            content: """{"intent":"order_status"}""",
            finishReason: LlmFinishReason.Stop,
            usage: usage);

        Assert.Equal("test-model", response.Model);
        Assert.Equal("""{"intent":"order_status"}""", response.Content);
        Assert.Equal(LlmFinishReason.Stop, response.FinishReason);
        Assert.Same(usage, response.Usage);
        Assert.Empty(response.ToolCalls);
        Assert.False(response.HasToolCalls);
    }

    [Fact]
    public void Constructor_WithEmptyModel_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmResponse(
                model: "",
                content: "response",
                finishReason: LlmFinishReason.Stop));
    }

    [Fact]
    public void Constructor_WithNeitherContentNorToolCalls_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmResponse(
                model: "test-model",
                content: null,
                finishReason: LlmFinishReason.ToolCall));
    }

    [Fact]
    public void Constructor_WithToolCallsAndNoContent_CreatesToolCallResponse()
    {
        var toolCall = new LlmToolCall(
            id: "call-1",
            name: "get_order_status",
            arguments:
            [
                new LlmToolCallArgument(
                    name: "orderId",
                    value: "ORD-1001")
            ]);

        var response = new LlmResponse(
            model: "test-model",
            content: null,
            finishReason: LlmFinishReason.ToolCall,
            toolCalls:
            [
                toolCall
            ]);

        Assert.Null(response.Content);
        Assert.True(response.HasToolCalls);
        Assert.Single(response.ToolCalls);
        Assert.Same(toolCall, response.ToolCalls[0]);
    }

    [Fact]
    public void Constructor_WithMultipleToolCalls_PreservesOrder()
    {
        var first = new LlmToolCall(
            id: "call-1",
            name: "get_order_status",
            arguments: []);

        var second = new LlmToolCall(
            id: "call-2",
            name: "get_customer_profile",
            arguments: []);

        var response = new LlmResponse(
            model: "test-model",
            content: null,
            finishReason: LlmFinishReason.ToolCall,
            toolCalls:
            [
                first,
                second
            ]);

        Assert.Equal(2, response.ToolCalls.Count);
        Assert.Same(first, response.ToolCalls[0]);
        Assert.Same(second, response.ToolCalls[1]);
    }

    [Fact]
    public void ToolCall_WithEmptyId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmToolCall(
                id: "",
                name: "get_order_status",
                arguments: []));
    }

    [Fact]
    public void ToolCall_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmToolCall(
                id: "call-1",
                name: "",
                arguments: []));
    }

    [Fact]
    public void ToolCallArgument_WithEmptyName_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmToolCallArgument(
                name: "",
                value: "ORD-1001"));
    }

    [Fact]
    public void ToolCallArgument_AllowsJsonValue()
    {
        var argument = new LlmToolCallArgument(
            name: "payload",
            value: """{"orderId":"ORD-1001","priority":true}""");

        Assert.Equal("payload", argument.Name);
        Assert.Equal("""{"orderId":"ORD-1001","priority":true}""", argument.Value);
    }
}
