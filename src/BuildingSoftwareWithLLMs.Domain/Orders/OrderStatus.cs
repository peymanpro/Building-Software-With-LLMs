namespace BuildingSoftwareWithLLMs.Domain.Orders;

public enum OrderStatus
{
    Processing,
    Shipped,
    Delivered,
    Delayed,
    Cancelled
}
