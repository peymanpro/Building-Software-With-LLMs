# Evaluation

The evaluation suite is intentionally deterministic.

## Purpose

The goal is not to measure model quality in the abstract. The goal is to verify that the software around an LLM behaves correctly:

- the provider contract is respected
- tool selection can be represented
- tool calls execute once per requested call
- tool output is returned to the conversation
- the orchestration loop terminates
- final application behavior remains stable across runs

## Dataset

The current evaluation cases cover:

| Prompt class | Expected tool calls | Expected result |
| --- | ---: | --- |
| General inquiry | 0 | general inquiry classification |
| Existing shipped order | 1 | Shipped |
| Existing delayed order | 1 | Delayed |
| Existing delivered order | 1 | Delivered |

The fake provider is used deliberately so that failures are attributable to application code rather than remote model variability.

## Extending the suite

Add a case when a new behavior becomes part of the application contract.

A useful evaluation case should identify:

- input prompt
- expected number of tool calls
- expected observable result
- the reason the behavior matters

When a real model is introduced, keep deterministic software-level evaluations and add a separate model-quality evaluation layer rather than replacing one with the other.
