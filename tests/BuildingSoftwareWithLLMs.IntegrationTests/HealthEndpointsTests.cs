using System.Net;
using BuildingSoftwareWithLLMs.IntegrationTests.Infrastructure;

namespace BuildingSoftwareWithLLMs.IntegrationTests;

public sealed class HealthEndpointsTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(ApiWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_Returns200AndHealthy()
    {
        using var response = await _client.GetAsync("/health");

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task Readiness_Returns200AndHealthy()
    {
        using var response = await _client.GetAsync("/health/ready");

        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", content);
    }
}
