using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Application.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

namespace BuildingSoftwareWithLLMs.EvaluationTests;

public sealed class LLMEvaluationTests
{
    public static IEnumerable<object[]> Cases()
    {
        yield return
        [
            "Hello there.",
            0,
            "general_inquiry"
        ];
        yield return
        [
            "Where is order ORD-1001?",
            1,
            "Shipped"
        ];
        yield return
        [
            "What happened to ORD-1002?",
            1,
            "Delayed"
        ];
        yield return
        [
            "Has order ORD-1003 arrived?",
            1,
            "Delivered"
        ];
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public async Task FakeProviderScenario_MatchesExpectedBehavior(
        string prompt,
        int expectedToolCalls,
        string expectedText)
    {
        var registry = new InMemoryLlmToolRegistry([new GetOrderStatusTool()]);
        var service = new LlmChatService(new FakeLlmProvider(), registry);

        var result = await service.ExecuteAsync(
            [new LlmMessage(LlmRole.User, prompt)],
            "fake-model");

        Assert.Equal(expectedToolCalls, result.ToolCallsExecuted);
        Assert.Contains(
            expectedText,
            result.Response.Content,
            StringComparison.OrdinalIgnoreCase);
    }
}
