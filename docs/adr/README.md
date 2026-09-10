# Architecture Decision Records

This directory contains Architecture Decision Records (ADRs) for decisions that materially affect the architecture, boundaries, reliability, security, evaluation, or operational behavior of the system.

## Principles

- Create an ADR only for a meaningful architectural decision.
- Record the context and problem before the decision.
- State the decision explicitly.
- Document important alternatives considered.
- Capture relevant consequences and trade-offs.
- Record limitations or follow-up implications when they materially matter.
- Do not create ADRs for routine implementation details.
- Once superseded, keep the original ADR and document the relationship rather than rewriting history.

## Status

Use one of:

- `Proposed`
- `Accepted`
- `Superseded`
- `Deprecated`

## Naming

Use a zero-padded sequence followed by a short kebab-case title:

`001-provider-abstraction.md`

The sequence is monotonic and should not be reused.

## Template

Start a new ADR from `000-template.md`.
