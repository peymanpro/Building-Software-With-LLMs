using BuildingSoftwareWithLLMs.Domain.Orders;

namespace BuildingSoftwareWithLLMs.UnitTests.Domain;

public sealed class OrderTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesOrder()
    {
        var order = new Order(
            "ORD-1001",
            OrderStatus.Shipped,
            "The order left the fulfillment center.");

        Assert.Equal("ORD-1001", order.Id);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(
            "The order left the fulfillment center.",
            order.StatusMessage);
    }

    [Fact]
    public void Constructor_WithEmptyId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Order("", OrderStatus.Processing, "Processing."));
    }

    [Fact]
    public void Constructor_WithEmptyStatusMessage_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Order("ORD-1001", OrderStatus.Processing, ""));
    }
}
