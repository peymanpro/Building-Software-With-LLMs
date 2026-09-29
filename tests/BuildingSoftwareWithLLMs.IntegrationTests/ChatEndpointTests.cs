using System.Net;
using System.Net.Http.Json;
using BuildingSoftwareWithLLMs.IntegrationTests.Infrastructure;

namespace BuildingSoftwareWithLLMs.IntegrationTests;

public sealed class ChatEndpointTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChatEndpointTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_OrderQuestion_RunsToolLoop()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/chat",
            new
            {
                prompt = "Where is order ORD-1002?"
            });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ChatResponse>();

        Assert.NotNull(body);
        Assert.Equal("fake-model", body!.Model);
        Assert.Contains("ORD-1002", body.Content);
        Assert.Contains("Delayed", body.Content);
        Assert.Equal(2, body.ProviderCalls);
        Assert.Equal(1, body.ToolCallsExecuted);
    }

    [Fact]
    public async Task GetTools_ListsRegisteredTools()
    {
        using var response = await _client.GetAsync("/api/tools");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tools = await response.Content.ReadFromJsonAsync<IReadOnlyList<ToolDefinition>>();
        Assert.NotNull(tools);
        Assert.Contains(tools!, tool => tool.Name == "get_order_status");
    }

    private sealed record ChatResponse(
        string Model,
        string? Content,
        string FinishReason,
        int ProviderCalls,
        int ToolCallsExecuted);

    private sealed record ToolDefinition(
        string Name,
        string Description,
        string ParametersJsonSchema);
}
