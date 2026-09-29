namespace BuildingSoftwareWithLLMs.Domain.Orders;

public sealed record Order
{
    public Order(
        string id,
        OrderStatus status,
        string statusMessage)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("Order id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            throw new ArgumentException(
                "Order status message cannot be empty.",
                nameof(statusMessage));
        }

        Id = id;
        Status = status;
        StatusMessage = statusMessage;
    }

    public string Id { get; }

    public OrderStatus Status { get; }

    public string StatusMessage { get; }
}
