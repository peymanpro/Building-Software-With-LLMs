# Architecture

## Responsibility map

### API

The API layer is the composition root. It wires implementations into the dependency injection container and exposes HTTP endpoints.

It should not contain LLM provider protocol details or tool business rules.

### Application

The Application layer owns the stable software-facing model:

- `ILLMProvider`
- `LlmRequest`
- `LlmResponse`
- `LlmMessage`
- tool definitions and tool calls
- `ILlmTool`
- `ILlmToolRegistry`
- `ILlmChatService`
- `LlmChatService`

This is where orchestration policy lives.

### Domain

The Domain layer contains concepts that exist independently of the LLM:

- `Order`
- `OrderStatus`

The LLM is an adapter to domain capabilities, not the owner of domain rules.

### Infrastructure

Infrastructure implements external and concrete mechanisms:

- deterministic fake model behavior
- OpenAI-compatible HTTP transport
- tool registry
- demo order lookup tool

## Orchestration loop

The orchestration service performs a bounded loop:

```text
1. Build LlmRequest with message history and available tools.
2. Call ILLMProvider.
3. Append the assistant message to history.
4. If there are no tool calls, finish.
5. Resolve each tool call through ILlmToolRegistry.
6. Execute each tool with cancellation.
7. Append tool result messages.
8. Repeat until a final response is produced or the provider-call limit is reached.
```

The limit prevents accidental infinite tool-call loops.

## Why the provider boundary is small

The provider contract represents application intent rather than a vendor SDK. Vendor-specific JSON serialization and transport stay in Infrastructure.

This makes provider behavior testable with an in-memory HTTP handler and allows the rest of the application to run against a deterministic fake.

## Tool boundary

A tool exposes two things:

1. A model-facing definition containing name, description, and JSON schema.
2. An application-facing execution method receiving structured arguments.

The model chooses a tool. The application remains responsible for validating, resolving, executing, and recording the result.

## Reliability considerations

Current protections include:

- argument validation on core contracts
- cancellation propagation
- bounded orchestration
- explicit unknown-tool errors
- deterministic local provider behavior
- HTTP error surfacing
- API health and readiness endpoints
- unit, integration, and evaluation tests

Production extensions can add retries, timeouts per operation, observability, policy checks, authorization, rate limits, durable conversation state, and stronger tool argument validation.
