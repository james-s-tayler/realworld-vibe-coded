# Skill Issue: Harness Engineering for Coding Agents

> Source: https://www.humanlayer.dev/blog/skill-issue-harness-engineering-for-coding-agents
> Author: Kyle (HumanLayer) | Date: March 12, 2026

## Core Thesis

Coding agent failures stem from poor configuration, not insufficient model capabilities. The formula: `coding agent = AI model(s) + harness`. The harness is everything the model uses to interact with its environment: skills, MCP servers, sub-agents, memory, and configuration files.

"Harness engineering describes the practice of leveraging configuration points to customize and improve your coding agent's output quality and reliability."

Harness engineering is a subset of context engineering, which itself extends beyond traditional prompt engineering.

## Configuration Surfaces

### CLAUDE.md / AGENTS.md (Agentfiles)

Markdown files at repository root that inject deterministically into the agent's system prompt.

**ETH Zurich Study findings:**
- LLM-generated agentfiles **hurt performance** while costing 20%+ more
- Human-written ones helped approximately 4%
- Agents spent 14-22% more reasoning tokens processing instructions
- Codebase overviews and directory listings provided **no benefit**

**Best practices:** Avoid auto-generation. Include minimal necessary instructions. Use progressive disclosure. Keep contents concise and universally applicable. HumanLayer's CLAUDE.md is under 60 lines.

### MCP Servers (Tools)

Extend agent capabilities beyond file I/O and bash. Tool descriptions are added to the system prompt.

**Warnings:**
- Tool descriptions present prompt injection risks
- Never connect to untrusted servers
- STDIO servers running client-side can execute code on host

**Best practices:**
- Avoid plugging in excessive MCP tools (creates context bloat)
- If duplicating CLI functionality already in training data, prompt agent to use CLI instead
- Anthropic's MCP tool search enables progressive disclosure
- Context-engineer custom CLIs with efficient responses

**Example:** HumanLayer replaced Linear MCP server with a custom CLI + 6 usage examples in CLAUDE.md, saving thousands of tokens per invocation.

### Skills (Reusable Knowledge via Progressive Disclosure)

Skills enable progressive disclosure — agents access specific instructions/tools only when needed.

**Security note:** Skill registries have distributed hundreds of malicious skills. Treat like `npm install random-package` — read before installing.

**Structure:**
```
example-skill/
|--- SKILL.md
|--- response_template.md
|--- CLIs/
    |--- linear-cli
    |--- tunnel-cli
```

When activated, `SKILL.md` loads into context as a user message. Can bundle multiple markdown files for different purposes.

### Sub-Agents (Context Control)

Not personality-based (frontend/backend/analyst agents don't work) — task-based encapsulation. Sub-agents act as "context firewalls," isolating discrete tasks in separate context windows so intermediate noise doesn't accumulate in the parent thread.

**Benefits:**
- Maintains coherency across many sessions
- Keeps primary agent thread in "smart zone"
- Enables use of cheaper models (Haiku/Sonnet) for sub-agents while using expensive models (Opus) for orchestration
- Cost control

**Context rot:** Chroma's research shows model performance degrades at longer context lengths. When low semantic similarity exists between questions and context, degradation is steeper. Sub-agents solve this structurally with fresh, small, high-relevance context windows.

**Use cases:** Locating definitions/implementations, analyzing codebase patterns, tracing information flow, general code/documentation/web research.

**Output pattern:** Condensed responses with source citations (filepath:line or URLs), enabling parent agent to verify without full context exposure.

### Hooks (Control Flow)

User-defined commands/scripts executing at agent lifecycle events. Run silently; return additional context alongside tool results; surface build/type errors before agent finishes.

**Use cases:**
- **Notifications** — sounds when finished, alerts for pending approvals
- **Approvals/Denials** — automatically approve/deny tool calls based on rules
- **Integrations** — Slack messages, GitHub PRs, preview environments
- **Verification** — run typecheck/build to surface errors

**Pattern:** Success is silent; only failures surface errors. Exit code 2 tells harness to re-engage agent for fixes.

**Example hook (TypeScript/Biome/Turbo):**
```bash
#!/bin/bash
cd "$CLAUDE_PROJECT_DIR"

PREBUILD_OUTPUT=$(bun run generate-cache-key && turbo run build --filter=@humanlayer/hld-sdk && bun install 2>&1)
if [ $? -ne 0 ]; then
  echo "prebuild failed:" >&2
  echo "$PREBUILD_OUTPUT" >&2
  exit 2
fi

OUTPUT=$(bun run --parallel \
  "biome check . --write --unsafe || biome check . --write --unsafe" \
  "turbo run typecheck" 2>&1)

if [ $? -ne 0 ]; then
  echo "$OUTPUT" >&2
  exit 2
fi
```

## Back-Pressure: Verification Mechanisms

Success correlates with the agent's ability to verify its own work.

**Mechanisms:**
- Typechecks and builds (preferably strongly-typed language)
- Unit/integration tests
- Code coverage reporting (hook can prompt for increased coverage if dropped)
- UI interaction/testing (Playwright, agent-browser)

**Critical principle:** Verification must be context-efficient. Early failures included running full test suites (4,000+ lines) flooding context and causing hallucination. **Solution: swallow successful output, surface only errors.** Same approach for builds.

## Post-Training Coupling

Models post-trained on specific harnesses (Claude on Claude Code, GPT-5 Codex on Codex) may perform better in their trained-on harnesses.

**Caveat:** Models can over-fit to harnesses. Terminal Bench 2.0 shows Opus 4.6 at #33 in Claude Code but #5 in a different harness (+/- ~4 positions).

## What Didn't Work

- Designing the ideal harness upfront before failures
- Installing dozens of skills/MCP servers "just in case"
- Running entire test suite (5+ minutes) after every session
- Micro-optimizing sub-agent tool access (creates tool thrash)

## What Did Work

- Starting simple, adding configuration only when failures occur
- Designing, testing, iterating, discarding unsuccessful approaches
- Distributing battle-tested configurations via repository config
- Optimizing iteration speed over first-attempt success
- Carefully paring down exposed capabilities after identifying needs

## Closing Insight

"The next time your coding agent isn't performing the way you expect, before you blame the model, check the harness. Agentfiles, MCP servers, skills, sub-agents, hooks, and back-pressure — that's where we've found most of the leverage. The model is probably fine. It's just a skill issue."
