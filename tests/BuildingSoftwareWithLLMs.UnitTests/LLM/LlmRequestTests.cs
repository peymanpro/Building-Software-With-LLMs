using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class LlmRequestTests
{
    [Fact]
    public void Constructor_WithValidInput_CreatesRequest()
    {
        var messages = new[]
        {
            new LlmMessage(LlmRole.System, "You are a support assistant."),
            new LlmMessage(LlmRole.User, "Where is my order?")
        };

        var request = new LlmRequest(
            model: "test-model",
            messages: messages,
            temperature: 0.2,
            maxOutputTokens: 256);

        Assert.Equal("test-model", request.Model);
        Assert.Equal(messages, request.Messages);
        Assert.Equal(0.2, request.Temperature);
        Assert.Equal(256, request.MaxOutputTokens);
    }

    [Fact]
    public void Constructor_WithEmptyModel_Throws()
    {
        var messages = new[]
        {
            new LlmMessage(LlmRole.User, "Hello")
        };

        Assert.Throws<ArgumentException>(() =>
            new LlmRequest("", messages));
    }

    [Fact]
    public void Constructor_WithNoMessages_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmRequest("test-model", Array.Empty<LlmMessage>()));
    }

    [Fact]
    public void Constructor_WithInvalidTemperature_Throws()
    {
        var messages = new[]
        {
            new LlmMessage(LlmRole.User, "Hello")
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LlmRequest("test-model", messages, temperature: 2.1));
    }

    [Fact]
    public void Constructor_WithInvalidMaxOutputTokens_Throws()
    {
        var messages = new[]
        {
            new LlmMessage(LlmRole.User, "Hello")
        };

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LlmRequest("test-model", messages, maxOutputTokens: 0));
    }

    [Fact]
    public void Message_WithEmptyContent_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new LlmMessage(LlmRole.User, ""));
    }
}
