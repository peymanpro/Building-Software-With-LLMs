using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.Infrastructure.LLM;

public sealed class OpenAiCompatibleLlmProvider : ILLMProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;

    public OpenAiCompatibleLlmProvider(
        HttpClient httpClient,
        OpenAiCompatibleLlmOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            throw new ArgumentException(
                "Base URL cannot be empty.",
                nameof(options));
        }

        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            options.BaseUrl.EndsWith("/", StringComparison.Ordinal)
                ? options.BaseUrl
                : options.BaseUrl + "/");
        _httpClient.Timeout = options.Timeout;

        if (!string.IsNullOrWhiteSpace(options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", options.ApiKey);
        }
    }

    public async Task<LlmResponse> CompleteAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var content = new StringContent(
            JsonSerializer.Serialize(CreateRequestBody(request), JsonOptions),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.PostAsync(
            "chat/completions",
            content,
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"LLM provider returned {(int)response.StatusCode} ({response.StatusCode}). " +
                $"Body: {responseBody}");
        }

        var payload = JsonSerializer.Deserialize<ChatCompletionResponse>(
            responseBody,
            JsonOptions)
            ?? throw new InvalidOperationException(
                "LLM provider returned an empty JSON response.");

        var choice = payload.Choices.FirstOrDefault()
            ?? throw new InvalidOperationException(
                "LLM provider returned no completion choices.");

        return new LlmResponse(
            model: payload.Model ?? request.Model,
            content: choice.Message.Content,
            finishReason: MapFinishReason(choice.FinishReason),
            usage: payload.Usage is null
                ? null
                : new LlmUsage(
                    payload.Usage.PromptTokens,
                    payload.Usage.CompletionTokens),
            toolCalls: ParseToolCalls(choice.Message.ToolCalls));
    }

    private static object CreateRequestBody(LlmRequest request) =>
        new
        {
            model = request.Model,
            messages = request.Messages.Select(ToWireMessage).ToArray(),
            temperature = request.Temperature,
            max_tokens = request.MaxOutputTokens,
            tools = request.Tools.Count == 0
                ? null
                : request.Tools.Select(tool => new
                {
                    type = "function",
                    function = new
                    {
                        name = tool.Name,
                        description = tool.Description,
                        parameters = JsonDocument.Parse(
                            tool.ParametersJsonSchema).RootElement.Clone()
                    }
                }).ToArray()
        };

    private static object ToWireMessage(LlmMessage message)
    {
        var wire = new Dictionary<string, object?>
        {
            ["role"] = ToWireRole(message.Role),
            ["content"] = message.Content
        };

        if (message.ToolCallId is not null)
        {
            wire["tool_call_id"] = message.ToolCallId;
        }

        if (message.Name is not null)
        {
            wire["name"] = message.Name;
        }

        if (message.HasToolCalls)
        {
            wire["tool_calls"] = message.ToolCalls.Select(toolCall => new
            {
                id = toolCall.Id,
                type = "function",
                function = new
                {
                    name = toolCall.Name,
                    arguments = JsonSerializer.Serialize(
                        toolCall.Arguments.ToDictionary(
                            argument => argument.Name,
                            argument => ParseJsonOrString(argument.Value)),
                        JsonOptions)
                }
            }).ToArray();
        }

        return wire;
    }

    private static object ParseJsonOrString(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            return value;
        }
    }

    private static IReadOnlyList<LlmToolCall> ParseToolCalls(
        IReadOnlyList<ToolCallResponse>? toolCalls)
    {
        if (toolCalls is null || toolCalls.Count == 0)
        {
            return [];
        }

        return toolCalls
            .Select(toolCall =>
                new LlmToolCall(
                    toolCall.Id,
                    toolCall.Function.Name,
                    ParseArguments(toolCall.Function.Arguments)))
            .ToArray();
    }

    private static IReadOnlyList<LlmToolCallArgument> ParseArguments(
        string? rawArguments)
    {
        if (string.IsNullOrWhiteSpace(rawArguments))
        {
            return [];
        }

        using var document = JsonDocument.Parse(rawArguments);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidOperationException(
                "LLM tool-call arguments must be a JSON object.");
        }

        return document.RootElement
            .EnumerateObject()
            .Select(property =>
                new LlmToolCallArgument(
                    property.Name,
                    property.Value.GetRawText()))
            .ToArray();
    }

    private static string ToWireRole(LlmRole role) =>
        role switch
        {
            LlmRole.System => "system",
            LlmRole.User => "user",
            LlmRole.Assistant => "assistant",
            LlmRole.Tool => "tool",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };

    private static LlmFinishReason MapFinishReason(string? finishReason) =>
        finishReason?.ToLowerInvariant() switch
        {
            "stop" => LlmFinishReason.Stop,
            "length" => LlmFinishReason.Length,
            "tool_calls" or "function_call" => LlmFinishReason.ToolCall,
            "content_filter" => LlmFinishReason.ContentFilter,
            _ => LlmFinishReason.Unknown
        };

    private sealed record ChatCompletionResponse(
        string? Model,
        IReadOnlyList<ChoiceResponse> Choices,
        UsageResponse? Usage);

    private sealed record ChoiceResponse(
        MessageResponse Message,
        [property: JsonPropertyName("finish_reason")] string? FinishReason);

    private sealed record MessageResponse(
        string? Content,
        [property: JsonPropertyName("tool_calls")] IReadOnlyList<ToolCallResponse>? ToolCalls);

    private sealed record ToolCallResponse(
        string Id,
        FunctionResponse Function);

    private sealed record FunctionResponse(
        string Name,
        string? Arguments);

    private sealed record UsageResponse(
        [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
        [property: JsonPropertyName("completion_tokens")] int CompletionTokens);
}
