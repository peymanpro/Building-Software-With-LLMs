using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class LlmResponseTests
{
    [Fact]
    public void Constructor_WithValidInput_CreatesResponse()
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
    public void Constructor_WithEmptyContent_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmResponse(
                model: "test-model",
                content: "",
                finishReason: LlmFinishReason.Stop));
    }

    [Fact]
    public void Usage_WithNegativeInputTokens_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LlmUsage(
                inputTokens: -1,
                outputTokens: 10));
    }

    [Fact]
    public void Usage_WithNegativeOutputTokens_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LlmUsage(
                inputTokens: 10,
                outputTokens: -1));
    }

    [Fact]
    public void Usage_TotalTokens_ReturnsSum()
    {
        var usage = new LlmUsage(
            inputTokens: 120,
            outputTokens: 45);

        Assert.Equal(165, usage.TotalTokens);
    }

    [Fact]
    public void Response_WithoutUsage_AllowsNullUsage()
    {
        var response = new LlmResponse(
            model: "test-model",
            content: "response",
            finishReason: LlmFinishReason.Unknown);

        Assert.Null(response.Usage);
    }
}
