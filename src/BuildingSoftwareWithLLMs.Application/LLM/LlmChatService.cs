using BuildingSoftwareWithLLMs.Application.Abstractions.LLM;

namespace BuildingSoftwareWithLLMs.Application.LLM;

public sealed class LlmChatService : ILlmChatService
{
    private const int DefaultMaxProviderCalls = 5;

    private readonly ILLMProvider _provider;
    private readonly ILlmToolRegistry _toolRegistry;
    private readonly int _maxProviderCalls;

    public LlmChatService(
        ILLMProvider provider,
        ILlmToolRegistry toolRegistry,
        int maxProviderCalls = DefaultMaxProviderCalls)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(toolRegistry);

        if (maxProviderCalls <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxProviderCalls),
                maxProviderCalls,
                "Maximum provider calls must be greater than zero.");
        }

        _provider = provider;
        _toolRegistry = toolRegistry;
        _maxProviderCalls = maxProviderCalls;
    }

    public async Task<LlmChatResult> ExecuteAsync(
        IReadOnlyList<LlmMessage> messages,
        string model,
        double? temperature = null,
        int? maxOutputTokens = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(messages);

        if (messages.Count == 0)
        {
            throw new ArgumentException(
                "At least one message is required.",
                nameof(messages));
        }

        var history = messages.ToList();
        var providerCalls = 0;
        var toolCallsExecuted = 0;

        while (providerCalls < _maxProviderCalls)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new LlmRequest(
                model,
                history,
                temperature,
                maxOutputTokens,
                _toolRegistry.Definitions);

            var response = await _provider.CompleteAsync(
                request,
                cancellationToken);

            providerCalls++;

            history.Add(new LlmMessage(
                LlmRole.Assistant,
                response.Content,
                response.ToolCalls));

            if (!response.HasToolCalls)
            {
                return new LlmChatResult(
                    response,
                    history,
                    providerCalls,
                    toolCallsExecuted);
            }

            foreach (var toolCall in response.ToolCalls)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_toolRegistry.TryResolve(toolCall.Name, out var tool))
                {
                    throw new LlmToolExecutionException(
                        $"No registered LLM tool exists with name '{toolCall.Name}'.");
                }

                try
                {
                    var toolOutput = await tool.ExecuteAsync(
                        toolCall.Arguments,
                        cancellationToken);

                    history.Add(new LlmMessage(
                        LlmRole.Tool,
                        toolOutput,
                        toolCallId: toolCall.Id,
                        name: toolCall.Name));

                    toolCallsExecuted++;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    throw new LlmToolExecutionException(
                        $"Tool '{toolCall.Name}' failed.",
                        exception);
                }
            }
        }

        throw new LlmToolExecutionException(
            $"LLM tool execution exceeded the maximum of {_maxProviderCalls} provider calls.");
    }
}
