using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

namespace BuildingSoftwareWithLLMs.UnitTests.LLM;

public sealed class InMemoryLlmToolRegistryTests
{
    [Fact]
    public void Constructor_BuildsDefinitionsAndCaseInsensitiveLookup()
    {
        var tool = new GetOrderStatusTool();
        var registry = new InMemoryLlmToolRegistry([tool]);

        Assert.Single(registry.Definitions);
        Assert.Equal("get_order_status", registry.Definitions[0].Name);
        Assert.True(registry.TryResolve("GET_ORDER_STATUS", out var resolved));
        Assert.Same(tool, resolved);
    }

    [Fact]
    public void Constructor_WithDuplicateNames_Throws()
    {
        var first = new GetOrderStatusTool();
        var second = new GetOrderStatusTool();

        Assert.Throws<ArgumentException>(() =>
            new InMemoryLlmToolRegistry([first, second]));
    }
}
