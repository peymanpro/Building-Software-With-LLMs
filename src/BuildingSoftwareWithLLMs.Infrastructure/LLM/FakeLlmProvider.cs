using System.Text.Json;
using System.Text.RegularExpressions;
using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.Infrastructure.LLM;

public sealed class FakeLlmProvider : ILLMProvider
{
    private static readonly Regex OrderIdPattern =
        new(@"\bORD-\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public Task<LlmResponse> CompleteAsync(
        LlmRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var latestToolMessage = request.Messages
            .LastOrDefault(message => message.Role == LlmRole.Tool);

        if (latestToolMessage is not null)
        {
            return Task.FromResult(CreateFinalResponse(
                request.Model,
                latestToolMessage.Content));
        }

        var latestUserMessage = request.Messages
            .LastOrDefault(message => message.Role == LlmRole.User)
            ?.Content ?? string.Empty;

        var canGetOrderStatus = request.Tools.Any(tool =>
            string.Equals(tool.Name, "get_order_status", StringComparison.OrdinalIgnoreCase));

        if (canGetOrderStatus &&
            (latestUserMessage.Contains("order", StringComparison.OrdinalIgnoreCase) ||
             OrderIdPattern.IsMatch(latestUserMessage)))
        {
            var match = OrderIdPattern.Match(latestUserMessage);
            var orderId = match.Success ? match.Value : "ORD-1001";

            var toolCall = new LlmToolCall(
                "call-1",
                "get_order_status",
                [
                    new LlmToolCallArgument("orderId", orderId.ToUpperInvariant())
                ]);

            return Task.FromResult(new LlmResponse(
                request.Model,
                null,
                LlmFinishReason.ToolCall,
                toolCalls: [toolCall]));
        }

        return Task.FromResult(new LlmResponse(
            request.Model,
            """{"intent":"general_inquiry","confidence":0.90}""",
            LlmFinishReason.Stop));
    }

    private static LlmResponse CreateFinalResponse(
        string model,
        string? toolContent)
    {
        var content = "The tool returned a result, but no readable status was found.";

        if (!string.IsNullOrWhiteSpace(toolContent))
        {
            try
            {
                using var document = JsonDocument.Parse(toolContent);
                var root = document.RootElement;

                if (root.TryGetProperty("found", out var found) &&
                    found.ValueKind == JsonValueKind.True)
                {
                    var orderId = root.GetProperty("orderId").GetString();
                    var status = root.GetProperty("status").GetString();
                    var message = root.GetProperty("message").GetString();

                    content = $"{orderId} is currently {status}. {message}";
                }
                else if (root.TryGetProperty("error", out var error) &&
                         error.TryGetProperty("message", out var errorMessage))
                {
                    content = errorMessage.GetString()
                        ?? "The order lookup failed.";
                }
            }
            catch (JsonException)
            {
                content = "The order lookup returned invalid tool output.";
            }
        }

        return new LlmResponse(
            model,
            content,
            LlmFinishReason.Stop);
    }
}
