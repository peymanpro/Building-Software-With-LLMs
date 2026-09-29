using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;
using Microsoft.AspNetCore.Mvc;

namespace BuildingSoftwareWithLLMs.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly ILlmChatService _chatService;
    private readonly IConfiguration _configuration;

    public ChatController(
        ILlmChatService chatService,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(chatService);
        ArgumentNullException.ThrowIfNull(configuration);

        _chatService = chatService;
        _configuration = configuration;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Post(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Prompt))
        {
            return BadRequest(new { error = "prompt is required" });
        }

        var model = string.IsNullOrWhiteSpace(request.Model)
            ? _configuration["Llm:DefaultModel"] ?? "fake-model"
            : request.Model;

        var messages = new List<LlmMessage>();

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            messages.Add(new LlmMessage(
                LlmRole.System,
                request.SystemPrompt));
        }

        messages.Add(new LlmMessage(
            LlmRole.User,
            request.Prompt));

        var result = await _chatService.ExecuteAsync(
            messages,
            model,
            request.Temperature,
            request.MaxOutputTokens,
            cancellationToken);

        return Ok(new ChatResponse(
            result.Response.Model,
            result.Response.Content,
            result.Response.FinishReason.ToString(),
            result.ProviderCalls,
            result.ToolCallsExecuted));
    }

    public sealed record ChatRequest(
        string Prompt,
        string? Model = null,
        string? SystemPrompt = null,
        double? Temperature = null,
        int? MaxOutputTokens = null);

    public sealed record ChatResponse(
        string Model,
        string? Content,
        string FinishReason,
        int ProviderCalls,
        int ToolCallsExecuted);
}
