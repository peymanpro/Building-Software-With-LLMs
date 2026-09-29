using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class OpenAiCompatibleLlmProviderTests
{
    [Fact]
    public async Task CompleteAsync_ParsesTextResponseAndUsage()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                model = "remote-model",
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "Hello from the model.",
                            tool_calls = (object[]?)null
                        },
                        finish_reason = "stop"
                    }
                },
                usage = new
                {
                    prompt_tokens = 12,
                    completion_tokens = 7
                }
            })
        });

        var provider = CreateProvider(handler);

        var response = await provider.CompleteAsync(
            new LlmRequest(
                "remote-model",
                [new LlmMessage(LlmRole.User, "Hello")]));

        Assert.Equal("remote-model", response.Model);
        Assert.Equal("Hello from the model.", response.Content);
        Assert.Equal(LlmFinishReason.Stop, response.FinishReason);
        Assert.NotNull(response.Usage);
        Assert.Equal(12, response.Usage!.InputTokens);
        Assert.Equal(7, response.Usage.OutputTokens);
    }

    [Fact]
    public async Task CompleteAsync_ParsesToolCalls()
    {
        var handler = new StubHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "model": "remote-model",
                  "choices": [
                    {
                      "message": {
                        "content": null,
                        "tool_calls": [
                          {
                            "id": "call-9",
                            "type": "function",
                            "function": {
                              "name": "get_order_status",
                              "arguments": "{"orderId":"ORD-1001"}"
                            }
                          }
                        ]
                      },
                      "finish_reason": "tool_calls"
                    }
                  ],
                  "usage": {
                    "prompt_tokens": 30,
                    "completion_tokens": 5
                  }
                }
                """)
        });

        var provider = CreateProvider(handler);

        var response = await provider.CompleteAsync(
            new LlmRequest(
                "remote-model",
                [new LlmMessage(LlmRole.User, "Where is ORD-1001?")],
                tools:
                [
                    new LlmToolDefinition(
                        "get_order_status",
                        "Returns order status.",
                        """{"type":"object","properties":{"orderId":{"type":"string"}},"required":["orderId"]}""")
                ]));

        var toolCall = Assert.Single(response.ToolCalls);
        Assert.Equal("call-9", toolCall.Id);
        Assert.Equal("get_order_status", toolCall.Name);
        Assert.Equal(LlmFinishReason.ToolCall, response.FinishReason);
        Assert.Equal("ORD-1001", Assert.Single(toolCall.Arguments).Value);
    }

    [Fact]
    public async Task CompleteAsync_SendsToolDefinitions()
    {
        HttpRequestMessage? capturedRequest = null;

        var handler = new StubHandler(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """{"model":"remote-model","choices":[{"message":{"content":"done"},"finish_reason":"stop"}]}""")
            };
        });

        var provider = CreateProvider(handler);

        await provider.CompleteAsync(
            new LlmRequest(
                "remote-model",
                [new LlmMessage(LlmRole.User, "Check order.")],
                tools:
                [
                    new LlmToolDefinition(
                        "get_order_status",
                        "Returns order status.",
                        """{"type":"object","properties":{"orderId":{"type":"string"}},"required":["orderId"]}""")
                ]));

        Assert.NotNull(capturedRequest);
        Assert.Equal(
            "https://example.test/v1/chat/completions",
            capturedRequest!.RequestUri!.ToString());

        using var json = JsonDocument.Parse(
            await capturedRequest.Content!.ReadAsStringAsync());

        var tool = json.RootElement
            .GetProperty("tools")[0]
            .GetProperty("function");

        Assert.Equal("get_order_status", tool.GetProperty("name").GetString());
        Assert.Equal(
            "Returns order status.",
            tool.GetProperty("description").GetString());
        Assert.Equal(
            "object",
            tool.GetProperty("parameters").GetProperty("type").GetString());
    }

    private static OpenAiCompatibleLlmProvider CreateProvider(
        HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);
        return new OpenAiCompatibleLlmProvider(
            client,
            new OpenAiCompatibleLlmOptions
            {
                BaseUrl = "https://example.test/v1/",
                ApiKey = "test-key"
            });
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}
