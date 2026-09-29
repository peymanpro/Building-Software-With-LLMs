# ADR 002: Use a bounded tool-execution loop

- Status: Accepted
- Date: 2026-09-29

## Context

Tool calling requires more than one provider interaction:

1. the model requests a tool
2. the application executes it
3. the tool result is sent back to the model
4. the model produces a final response

A loop without a bound could repeatedly execute model-requested tools.

## Decision

`LlmChatService` performs tool execution in a bounded provider-call loop.

The current default maximum is five provider calls per execution.

## Alternatives Considered

### Single provider call

This cannot complete a normal tool-call round trip.

### Unbounded loop

This creates an avoidable failure mode if a provider repeatedly requests tools.

## Consequences

Normal tool-calling conversations can complete without special-case controller logic.

A provider that never returns a final response eventually fails with an explicit orchestration error.

## Trade-offs

The bound is simple and predictable, but a future workflow with intentionally long multi-step reasoning may need a configurable or policy-driven limit.

## Limitations

The current implementation does not persist intermediate execution state across process boundaries.
