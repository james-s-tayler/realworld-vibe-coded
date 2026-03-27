# Ralph Wiggum as a "Software Engineer"

> Source: https://ghuntley.com/ralph/

## Core Concept

Geoffrey Huntley presents "Ralph" -- a technique for autonomous AI-driven software development using iterative bash loops that invoke Claude Code. The method demonstrates significant efficiency gains, with one engineer completing a $50k contract for approximately $297 in costs.

Ralph operates as a simple bash loop:

```bash
while :; do cat PROMPT.md | claude-code ; done
```

The technique enables AI to autonomously build software through repeated cycles, making decisions about implementation priorities and self-correcting through tuned prompts.

## Key Principles

**Monolithic Architecture**: Ralph operates as a single repository process rather than distributed microservices, avoiding complexity from non-deterministic agent communication.

**One Task Per Loop**: Each iteration focuses exclusively on implementing one feature, maintaining context efficiency across the 170k token budget.

**Deterministic Stack Allocation**: Specifications and planning documents get reloaded every loop, providing consistent guidance.

**Subagent Parallelism**: Up to 500 parallel subagents can search codebases and write files, with strict serialization for testing and builds.

## Implementation Stages

**Phase One -- Generate**: Code production is now inexpensive; quality depends on specifications and standard libraries controlling output patterns.

**Phase Two -- Backpressure**: Engineers implement validation layers including testing, type checking, security scanning, and static analysis to reject invalid generations.

## Critical Practices

- Search thoroughly before assuming code isn't implemented
- Write tests capturing implementation rationale for future loops
- Avoid placeholder implementations through explicit prompting
- Maintain TODO lists identifying incomplete work
- Update documentation explaining test importance and design decisions
- Keep learning logs in dedicated files for loop-to-loop continuity

## Production Example: CURSED

Huntley uses Ralph to build a new esoteric programming language with compiler, standard library, and LLVM integration -- demonstrating an AI system creating tools without those tools appearing in training data.

## Critical Perspective

"There's no way in heck would I use Ralph in an existing code base" -- the technique works best for greenfield projects targeting approximately 90% completion. Engineers remain essential for guidance, even if the execution becomes largely autonomous.
