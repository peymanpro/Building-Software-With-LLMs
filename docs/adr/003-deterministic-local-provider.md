# ADR 003: Keep a deterministic fake provider for local evaluation

- Status: Accepted
- Date: 2026-09-29

## Context

Network access, API keys, latency, provider outages, and model variability are poor prerequisites for basic software tests.

The repository needs repeatable tests that validate orchestration and tool behavior.

## Decision

Maintain a deterministic `FakeLlmProvider` that implements the same `ILLMProvider` contract as a real provider.

## Alternatives Considered

### Call a real model in every test

This would make tests slower, non-deterministic, and dependent on external credentials.

### Mock the orchestration service itself

This would test the wrong boundary: the orchestration behavior would no longer be exercised.

## Consequences

The application can validate end-to-end tool-call behavior without network access.

Evaluation failures point to application changes more directly.

## Trade-offs

The fake provider is a behavioral test double, not evidence of model quality.

## Limitations

A passing fake-provider evaluation does not establish that a real model will choose tools correctly. Real-provider and model-quality evaluations remain separate concerns.
