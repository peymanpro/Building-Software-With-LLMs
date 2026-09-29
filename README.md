# Building Software With LLMs

A small, production-shaped .NET 8 reference application for building software around large language models without coupling the application core to a specific model vendor.

The project demonstrates a complete path from a user message to an LLM response:

`Request → Provider → Tool Call → Tool Execution → Tool Result → Final Response`

It deliberately keeps the LLM boundary behind application contracts so that a deterministic fake provider can be used for development and evaluation, while an OpenAI-compatible HTTP provider can be used for a real model endpoint.

## What is implemented

- Provider-agnostic LLM request/response contracts.
- System, user, assistant, and tool messages.
- Structured tool definitions and tool-call arguments.
- A bounded orchestration loop with cancellation and unknown-tool handling.
- A deterministic `FakeLlmProvider` for repeatable tests.
- An OpenAI-compatible `/chat/completions` HTTP adapter.
- An in-memory tool registry.
- A domain-level order model and a `get_order_status` tool.
- ASP.NET Core endpoints for chat and tool discovery.
- Unit, integration, and evaluation tests.
- Architecture Decision Records and GitHub Actions CI.

## Architecture

```text
                    ┌─────────────────────┐
                    │    ASP.NET Core     │
                    │ ChatController      │
                    └──────────┬──────────┘
                               │
                               ▼
                    ┌─────────────────────┐
                    │   LlmChatService    │
                    │ orchestration loop  │
                    └──────┬───────┬──────┘
                           │       │
                  ILLMProvider     │ ILlmToolRegistry
                           │       │
             ┌─────────────┘       └──────────────┐
             ▼                                    ▼
 ┌────────────────────────┐          ┌────────────────────────┐
 │ FakeLlmProvider        │          │ GetOrderStatusTool      │
 │ deterministic          │          │ domain-backed           │
 └────────────────────────┘          └────────────────────────┘
             │
             ▼
 ┌────────────────────────┐
 │ OpenAiCompatible       │
 │ HTTP Provider           │
 └────────────────────────┘
```

The Application layer owns the contracts and orchestration policy. Infrastructure supplies providers and tools. The API is the composition root.

## Example flow

Request:

```json
{
  "prompt": "Where is order ORD-1002?"
}
```

The fake provider selects `get_order_status`, the tool returns deterministic demo data, and the orchestration loop sends the tool result back to the provider.

A representative response is:

```json
{
  "model": "fake-model",
  "content": "ORD-1002 is currently Delayed. The carrier reported a delivery delay.",
  "finishReason": "Stop",
  "providerCalls": 2,
  "toolCallsExecuted": 1
}
```

## Run locally

The solution targets .NET 8 and pins the SDK through `global.json`.

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
dotnet run --project src/BuildingSoftwareWithLLMs.Api
```

The API exposes Swagger in the Development environment.

### Demo endpoints

`POST /api/chat`

```json
{
  "prompt": "Where is order ORD-1002?"
}
```

`GET /api/tools` lists the registered tool definitions.

`GET /health` and `GET /health/ready` expose application health.

## Using a real model endpoint

Set:

```text
Llm:Provider=OpenAICompatible
Llm:DefaultModel=<your-model>
Llm:OpenAICompatible:BaseUrl=https://your-endpoint/v1/
Llm:OpenAICompatible:ApiKey=<secret>
```

The adapter intentionally uses `HttpClient` and the provider contract instead of leaking a vendor SDK into the Application layer.

Never commit API keys or other secrets. Use environment variables, user secrets, or another secret store.

## Testing strategy

The test suite is split by purpose:

- Unit tests protect contracts, validation, providers, registries, and orchestration behavior.
- Integration tests exercise the HTTP API through `WebApplicationFactory`.
- Evaluation tests run deterministic scenarios against the fake provider so behavior can be checked without network access or model drift.

The current evaluation set covers general inquiries plus shipped, delayed, and delivered order states.

## Design boundaries

This repository is intentionally small. It does not pretend that a fake provider is equivalent to a production LLM, and it does not hide provider-specific behavior behind a leaky abstraction.

The important engineering boundary is the contract:

```csharp
Task<LlmResponse> CompleteAsync(
    LlmRequest request,
    CancellationToken cancellationToken = default);
```

Everything around the model can therefore be tested deterministically.

## Project layout

```text
src/
  BuildingSoftwareWithLLMs.Api/
  BuildingSoftwareWithLLMs.Application/
    Abstractions/LLM/
    LLM/
  BuildingSoftwareWithLLMs.Domain/
    Orders/
  BuildingSoftwareWithLLMs.Infrastructure/
    LLM/

tests/
  BuildingSoftwareWithLLMs.UnitTests/
  BuildingSoftwareWithLLMs.IntegrationTests/
  BuildingSoftwareWithLLMs.EvaluationTests/

docs/
  adr/
```

## Current completion state

The repository has moved beyond the initial contract-only baseline and now contains an end-to-end, locally testable LLM application path, including tool calling, a real HTTP provider seam, API exposure, evaluation coverage, documentation, and CI.

See `docs/adr/` for the architectural decisions behind the main boundaries.
