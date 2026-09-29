using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using BuildingSoftwareWithLLMs.Application.LLM;
using BuildingSoftwareWithLLMs.Infrastructure.LLM;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

builder.Services.AddScoped<ILlmChatService, LlmChatService>();
builder.Services.AddSingleton<GetOrderStatusTool>();
builder.Services.AddSingleton<ILlmTool>(
    serviceProvider => serviceProvider.GetRequiredService<GetOrderStatusTool>());
builder.Services.AddSingleton<ILlmToolRegistry>(
    serviceProvider => new InMemoryLlmToolRegistry(
        serviceProvider.GetServices<ILlmTool>()));

var provider = builder.Configuration["Llm:Provider"] ?? "Fake";

if (string.Equals(
    provider,
    "OpenAICompatible",
    StringComparison.OrdinalIgnoreCase))
{
    var options = new OpenAiCompatibleLlmOptions
    {
        BaseUrl = builder.Configuration["Llm:OpenAICompatible:BaseUrl"]
            ?? "https://api.openai.com/v1/",
        ApiKey = builder.Configuration["Llm:OpenAICompatible:ApiKey"]
            ?? string.Empty
    };

    builder.Services.AddSingleton(options);
    builder.Services.AddHttpClient<OpenAiCompatibleLlmProvider>();
    builder.Services.AddTransient<ILLMProvider>(
        serviceProvider => serviceProvider
            .GetRequiredService<OpenAiCompatibleLlmProvider>());
}
else
{
    builder.Services.AddSingleton<ILLMProvider, FakeLlmProvider>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.MapControllers();

app.Run();

public partial class Program
{
}
