# ADR 001: Keep the LLM provider behind an application contract

- Status: Accepted
- Date: 2026-09-29

## Context

The application needs to call an LLM while remaining testable and independent of a specific provider implementation.

The initial repository already contained a small `ILLMProvider` contract, but the rest of the application did not yet use it.

## Decision

Keep provider interaction behind `ILLMProvider` and represent requests and responses with application-owned contracts.

Provider-specific transport and wire formats belong in Infrastructure.

## Alternatives Considered

### Vendor SDK throughout the application

This would reduce the amount of adapter code but make application behavior dependent on a vendor-specific object model.

### Direct HTTP calls from controllers

This would make the API layer responsible for protocol details and make deterministic testing harder.

## Consequences

The Application layer can be tested with `FakeLlmProvider`.

The Infrastructure layer can replace or extend providers without changing the orchestration contract.

The adapter must translate between provider wire format and application objects.

## Trade-offs

The abstraction has to model the subset of provider behavior the application actually uses. It should grow from real requirements rather than attempt to model every provider feature in advance.

## Limitations

The current provider contract does not model streaming, multimodal content, log probabilities, or provider-specific metadata.
