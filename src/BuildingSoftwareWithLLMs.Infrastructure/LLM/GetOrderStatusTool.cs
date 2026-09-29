using System.Text.Json;
using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Domain.Orders;

namespace BuildingSoftwareWithLLMs.Infrastructure.LLM;

public sealed class GetOrderStatusTool : ILlmTool
{
    private static readonly IReadOnlyDictionary<string, Order> Orders =
        new Dictionary<string, Order>(StringComparer.OrdinalIgnoreCase)
        {
            ["ORD-1001"] = new(
                "ORD-1001",
                OrderStatus.Shipped,
                "The order left the fulfillment center."),
            ["ORD-1002"] = new(
                "ORD-1002",
                OrderStatus.Delayed,
                "The carrier reported a delivery delay."),
            ["ORD-1003"] = new(
                "ORD-1003",
                OrderStatus.Delivered,
                "The order was delivered to the customer.")
        };

    public LlmToolDefinition Definition { get; } =
        new(
            "get_order_status",
            "Returns the current status of a customer order.",
            """
            {
              "type": "object",
              "properties": {
                "orderId": {
                  "type": "string",
                  "description": "Customer order identifier, for example ORD-1001."
                }
              },
              "required": ["orderId"],
              "additionalProperties": false
            }
            """);

    public Task<string> ExecuteAsync(
        IReadOnlyList<LlmToolCallArgument> arguments,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        cancellationToken.ThrowIfCancellationRequested();

        var orderId = arguments
            .FirstOrDefault(argument =>
                string.Equals(
                    argument.Name,
                    "orderId",
                    StringComparison.OrdinalIgnoreCase))
            ?.Value;

        if (string.IsNullOrWhiteSpace(orderId))
        {
            return Task.FromResult(SerializeError(
                "missing_order_id",
                "The orderId argument is required."));
        }

        orderId = orderId.Trim();

        if (!Orders.TryGetValue(orderId, out var order))
        {
            return Task.FromResult(SerializeError(
                "order_not_found",
                $"No order named '{orderId}' exists in the demo data."));
        }

        return Task.FromResult(JsonSerializer.Serialize(new
        {
            found = true,
            orderId = order.Id,
            status = order.Status.ToString(),
            message = order.StatusMessage
        }));
    }

    private static string SerializeError(string code, string message) =>
        JsonSerializer.Serialize(new
        {
            found = false,
            error = new
            {
                code,
                message
            }
        });
}
