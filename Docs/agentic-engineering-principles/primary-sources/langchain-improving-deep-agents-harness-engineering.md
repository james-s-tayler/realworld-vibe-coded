# Improving Deep Agents with Harness Engineering

> Source: https://blog.langchain.com/improving-deep-agents-with-harness-engineering/

## Overview

LangChain's coding agent improved dramatically on Terminal Bench 2.0, jumping from Top 30 to Top 5 by refining only the harness while maintaining the same underlying model. The team increased performance from 52.8% to 66.5% through systematic optimization.

## The Goal of Harness Engineering

Harness engineering focuses on building systems around models to optimize task performance, token efficiency, latency, and other objectives. Rather than changing the model itself, engineers adjust system prompts, tool selection, and execution flows. The approach uses traces to identify failure patterns at scale.

## Optimization Framework

The team focused on three primary adjustable components:
- System prompts
- Tools available to the agent
- Middleware (hooks around model and tool calls)

They also created a "Trace Analyzer Skill" that automates error analysis across runs, functioning similarly to boosting by focusing improvement efforts on previous mistakes.

## Key Improvements

### Self-Verification Loop

Models naturally stop after generating initial solutions without testing. The team implemented a structured four-phase approach:

1. **Planning & Discovery** - Understanding the task and planning verification
2. **Build** - Implementation with testing in mind
3. **Verify** - Running tests against specifications
4. **Fix** - Addressing identified issues

A `PreCompletionChecklistMiddleware` forces verification passes before agent exit.

### Environmental Context

Agents perform better when given explicit information about their working environment. Improvements included:

- **Directory mapping** through `LocalContextMiddleware` that discovers the workspace structure and available tools
- **Testing standards** - Prompting agents that solutions will face automated verification
- **Time budgeting** - Warning agents about deadline constraints

## Pattern Detection

A `LoopDetectionMiddleware` tracks file edits and alerts agents when they're stuck repeating unsuccessful approaches, helping them reconsider strategies.

## Reasoning Budget Allocation

The team tested different configurations of reasoning modes. They found a "reasoning sandwich" approach -- using highest reasoning for planning and verification while using standard reasoning for implementation -- balanced performance and computational efficiency. Running maximum reasoning throughout resulted in timeouts.

## Practical Principles

1. **Context assembly** - Preparing information about environments, tools, and best practices reduces agent errors
2. **Verification focus** - Aggressive prompting for testing and refinement improves autonomous systems
3. **Trace-driven debugging** - Analyzing execution traces reveals both reasoning and tooling issues
4. **Pattern mitigation** - Current guardrails address present model limitations while planning for future improvements
5. **Model-specific tuning** - Different models require tailored harness approaches
