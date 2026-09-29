using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Application.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingSoftwareWithLLMs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLlmInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddScoped<ILlmChatService, LlmChatService>();

        services.AddSingleton<GetOrderStatusTool>();
        services.AddSingleton<ILlmTool>(
            serviceProvider => serviceProvider.GetRequiredService<GetOrderStatusTool>());

        services.AddSingleton<ILlmToolRegistry>(
            serviceProvider => new InMemoryLlmToolRegistry(
                serviceProvider.GetServices<ILlmTool>()));

        var provider = configuration["Llm:Provider"] ?? "Fake";

        if (string.Equals(
            provider,
            "OpenAICompatible",
            StringComparison.OrdinalIgnoreCase))
        {
            var options = new OpenAiCompatibleLlmOptions
            {
                BaseUrl = configuration["Llm:OpenAICompatible:BaseUrl"]
                    ?? "https://api.openai.com/v1/",
                ApiKey = configuration["Llm:OpenAICompatible:ApiKey"]
                    ?? string.Empty
            };

            services.AddSingleton(options);
            services.AddHttpClient<OpenAiCompatibleLlmProvider>();

            services.AddTransient<ILLMProvider>(
                serviceProvider => serviceProvider
                    .GetRequiredService<OpenAiCompatibleLlmProvider>());
        }
        else
        {
            services.AddSingleton<ILLMProvider, FakeLlmProvider>();
        }

        return services;
    }
}
