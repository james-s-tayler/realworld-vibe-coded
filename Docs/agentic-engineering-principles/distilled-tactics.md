# Distilled Tactics for Harness Engineering

Concrete, proven tactics extracted from 9 primary sources. Each tactic has been implemented by practitioners and has reported results. Organized by the problem they solve.

---

## Context Management

### Frequent Intentional Compaction

Split all work into Research → Plan → Implement with explicit context resets between phases. Keep context utilization at 40-60%. After each implementation phase, compact current status back into the plan file before continuing.

**Reported results:** 35k LOC shipped in 7 hours across a 300k LOC Rust codebase. Estimated 3-5 days per feature for a senior engineer. *(ACE)*

### Subagents for Search and Summarize

Spawn fresh-context subagents to handle all searching, file reading, and exploration. Parent agent receives only a focused markdown summary with key findings. Never pollute the parent context with Glob/Grep/Read noise.

Subagents are about context control, not role-playing — personality-based agents (frontend/backend/analyst) don't work. The ideal output is a condensed markdown artifact with findings, next steps, and source citations (filepath:line or URLs) so the parent can verify without full context exposure. Use cheaper models (Haiku/Sonnet) for subagents while reserving expensive models (Opus) for orchestration. *(ACE, Ralph, HumanLayer)*

### One Task Per Loop

Each agent session or loop iteration focuses exclusively on implementing one feature. Reload specifications and planning documents at the start of every iteration. This maintains context efficiency within the token budget.

```bash
while :; do cat PROMPT.md | claude-code ; done
```

**Reported results:** $50k contract completed for ~$297 in compute costs. *(Ralph)*

### Commit Messages as Compaction

Use detailed git commit messages to distill and preserve context state before ending a session. The commit message becomes the bridge between sessions. *(ACE)*

### Reasoning Sandwich

Use highest reasoning mode for planning and verification phases. Use standard reasoning for implementation. Running maximum reasoning throughout causes timeouts without proportional quality improvement.

**Reported results:** Balanced performance and computational efficiency on Terminal Bench 2.0. *(LangChain)*

---

## Workflow Structure

### Three-Phase Pipeline: Research → Plan → Implement

**Research:** Understand the codebase, map relevant files, trace information flow, identify potential causes. Output: a research markdown document validated against FAR criteria (Factual, Actionable, Relevant).

**Plan:** Outline exact steps, specify files to edit and how, detail testing and verification for each phase. Validate each task against FACTS criteria (Feasible, Atomic, Clear, Testable, Scoped).

**Implement:** Execute the plan phase by phase. Compact status into the plan after each verified phase. Only this step needs a worktree; research and planning happen on main.

**Reported results:** Plan built with research fixed the bug in the best place with testing aligned to codebase conventions. Plan without research "would have worked" but in a suboptimal way. *(ACE, RPI)*

### Reverse Prompting (Research Phase)

During research, have the agent ask clarifying questions one at a time rather than accepting a vague spec. "Should this work from the file manager or dashboard?" "Any file type restrictions?" "What happens to shared files?" This reveals hidden requirements before any code is written. *(RPI)*

### Feedback Loop Granularity

Choose validation frequency to match task complexity:
- **"Do a task, validate"** — maximum control for risky changes
- **"Do a phase, validate"** — balanced speed for well-understood work
- **"Do the whole thing, validate"** — when confidence is high

*(RPI)*

### Session Startup Checklist

Every new session begins with:
1. Read current directory and progress files
2. Review git history
3. Run basic functionality tests
4. Select next unfinished feature

This saves tokens and immediately identifies broken states requiring repair before new work begins. *(Anthropic)*

### Initializer Agent Pattern

First session creates foundational artifacts:
- `init.sh` script for environment setup
- `claude-progress.txt` documenting what the agent has done
- Initial git commits establishing file structure

Subsequent sessions read these artifacts to resume without redoing setup. *(Anthropic)*

---

## Validation and Quality

### Generate → Backpressure Pipeline

Decouple generation from validation into two explicit phases:
- **Phase 1 — Generate:** Produce code; quality depends on specifications and standard library patterns
- **Phase 2 — Backpressure:** Reject invalid generations through tests, type checks, linters, security scanning, and static analysis

Quality emerges from selection pressure, not from single-shot perfection.

**Critical:** Verification output must be context-efficient. Running full test suites floods context with 4,000+ lines and causes hallucination. **Swallow successful output; surface only errors.** Same approach for builds and typechecks. *(Ralph, HumanLayer)*

### Self-Verification Loop

Force a four-phase verification cycle before any task is marked complete:
1. **Planning & Discovery** — understand the task, plan verification approach
2. **Build** — implement with testing in mind
3. **Verify** — run tests against specifications
4. **Fix** — address identified issues

Implement as `PreCompletionChecklistMiddleware` that blocks agent exit until verification passes. *(LangChain)*

### Generator-Evaluator Architecture

Separate the work-generating agent from the evaluating agent. Agents confidently praise their own mediocre work. Independent evaluators, tuned for appropriate skepticism, catch problems the generator cannot see in itself.

**Reported results:** Solo agent: 20 min, $9. Full harness: 6 hours, $200. The harness-generated application was substantially more functional and polished. *(Anthropic)*

### Chrome DevTools MCP Validation Loop

Drive the application through Chrome DevTools Protocol, not just code inspection:
1. Select target and clear console
2. Snapshot BEFORE interaction
3. Trigger UI path
4. Capture runtime events
5. Snapshot AFTER interaction
6. Apply fixes and restart
7. Re-run validation until clean

**Why it matters:** Code inspection cannot catch runtime bugs. Full-stack validation requires actual user-path interaction. *(OpenAI)*

### Design Evaluation Grading Criteria

Make subjective quality gradable through explicit dimensions:
- **Design quality:** Coherent aesthetic identity (colors, typography, layout)
- **Originality:** Evidence of custom decisions vs. template defaults
- **Craft:** Technical execution (spacing, typography, contrast)
- **Functionality:** Usability and task completion without confusion

Note: specific language in criteria descriptions steers output. "Museum quality" produced a different aesthetic than "professional quality." *(Anthropic)*

### Feature List JSON (200+ Items)

Maintain a JSON file with 200+ detailed features, each with specific implementation steps and pass/fail status. This prevents premature completion ("I'm done!"), provides explicit next-step guidance, and creates a persistent progress tracker across sessions. *(Anthropic)*

---

## Tool and Architecture Design

### Minimal Tool Surface

Strip sophisticated multi-tool systems down to the minimum. Grant direct access to well-structured data via standard utilities (grep, cat, find on YAML/Markdown/JSON).

**Reported results:** 3.5x faster (77s vs 275s), 100% success (up from 80%), 37% fewer tokens, 42% fewer steps.

**Critical prerequisite:** Foundation must be well-structured and consistently named. Without quality underlying data, simplification produces faster bad results. *(Vercel)*

### Architecture Enforcement via Mechanical Boundaries

Don't document architectural rules — enforce them:
- Custom linters (code-generated) validate dependency directions
- Strict layered domains with explicit cross-cutting boundaries
- Type system enforces data shapes at boundaries
- Compilation fails if agent violates boundaries

Encouragement-only approaches (comments, docs) do not work at scale. *(OpenAI)*

### Progressive Disclosure via Lightweight Entry Point

Keep the injected instruction file small (~100 lines). Use it as a table of contents with pointers to deeper sources of truth in a structured docs/ directory.

**Why:** Too much guidance becomes non-guidance. Agents pattern-match locally on giant instruction blobs instead of navigating intentionally. Monolithic manuals become graveyards of stale rules. ETH Zurich confirmed: LLM-generated agentfiles hurt performance while costing 20%+ more; codebase overviews and directory listings provided no benefit. *(OpenAI, HumanLayer)*

### Replace MCP Servers with Custom CLIs

When an MCP server duplicates CLI functionality already in the model's training data, replace the MCP server with the CLI and add usage examples to CLAUDE.md. MCP tool descriptions inject into the system prompt, creating per-invocation context bloat.

**Example:** HumanLayer replaced their Linear MCP server with a custom CLI + 6 usage examples in CLAUDE.md, saving thousands of tokens per invocation. *(HumanLayer)*

### Silent Success, Loud Failure Hooks

Design hooks so successful operations produce no output (preserving context budget) and only failures surface diagnostic information. Use exit code 2 to block the agent and re-engage it for fixes.

```bash
#!/bin/bash
OUTPUT=$(some-build-or-lint-command 2>&1)
if [ $? -ne 0 ]; then
  echo "$OUTPUT" >&2
  exit 2
fi
# Success: silent. No context consumed.
```

This pattern maximizes the verification frequency you can afford within the context budget. *(HumanLayer)*

### Skills as Progressive Disclosure Bundles

Package domain-specific instructions, response templates, and CLIs into skill directories that only load when activated. This keeps the baseline context small while making deep expertise available on demand.

```
example-skill/
|--- SKILL.md           # Instructions loaded on activation
|--- response_template.md
|--- CLIs/
    |--- custom-cli
```

**Security warning:** Skill registries have distributed malicious skills. Always read skill source before installing — treat like `npm install random-package`. *(HumanLayer)*

### Full Observability Stack

Provide agents with queryable observability:
- HTTP logs, OTLP metrics, OTLP traces → Vector → Victoria Logs/Metrics/Traces
- LogQL, PromQL, TraceQL APIs available to the agent
- Agent can query, correlate, and reason about runtime behavior

This enables a feedback loop where the agent diagnoses issues from production telemetry, not just source code. *(OpenAI)*

---

## Failure Detection and Recovery

### Loop Detection Middleware

Track file edits across turns. When the agent is repeating the same unsuccessful approach, alert it and prompt strategy reconsideration. Without this, agents get stuck in edit loops indefinitely. *(LangChain)*

### Trace-Driven Debugging

Build a Trace Analyzer that automatically categorizes failures across runs. This functions like boosting: each improvement cycle focuses on the mistakes from the previous round rather than re-examining all behavior.

**Reported results:** Terminal Bench 2.0 improvement from Top 30 to Top 5 (52.8% → 66.5%) using the same underlying model. *(LangChain)*

### Environmental Context Assembly

Proactively provide agents with explicit information about their working environment before they start:
- Directory structure mapping via middleware
- Available tools and their capabilities
- Testing standards ("your solution will face automated verification")
- Time budget constraints ("you have N minutes remaining")

Agents make fewer errors when they know their environment explicitly rather than discovering it through trial and error. *(LangChain)*

---

## Knowledge Management

### Specs as Source of Truth

Treat specifications and plans as the canonical artifacts, not the generated code. Code is a compiled output; specs are the source. This enables:
- Readable 200-line plans instead of unreadable 2000-line PRs
- Mental alignment across teams without reading all code
- Regeneration of code from specs when needed

**Reported results:** 6 PRs shipped per day. Fewer than 5 manual file edits in 3 months. *(ACE)*

### Agent-Legible Repository Structure

Structure the repository for agent discovery, not just human reading. Everything the agent needs must be on disk, not in external systems (Google Docs, Slack, Confluence). The repository's docs/ directory is the system of record.

**Key constraint:** Agent knowledge = what the agent can see. Unseen knowledge does not exist to the agent. *(OpenAI)*

### Knowledge Base Freshness Validation

Use CI jobs and automated checks to validate:
- Knowledge base documents are up to date
- Cross-links between documents are valid
- Coverage, freshness, and ownership metadata are correct

Without automated freshness checks, knowledge bases decay into misleading artifacts. *(OpenAI)*

### Learning Logs for Cross-Session Continuity

Maintain dedicated files where the agent records what it learned during each session — what worked, what failed, what patterns were discovered. Future sessions read these logs to avoid repeating mistakes. *(Ralph)*

---

## Human-Agent Collaboration

### High-Leverage Review Points

Focus human review on research findings and implementation plans, not on generated code. The leverage hierarchy:
- Bad research → thousands of bad lines of code
- Bad plan → hundreds of bad lines of code
- Bad code → a bad line of code

Review the research. Review the plan. Spot-check the code. *(ACE)*

### Minimal Merge Gates

In high-throughput agent systems, don't block PRs until they're perfect. Keep PRs short-lived. Address failures with follow-up PRs rather than blocking indefinitely. The operating principle: fix fast, almost never "try harder." *(OpenAI)*

### Model Capability Assumption Stress Testing

Every harness component encodes an assumption about what the model cannot do alone. As models improve (4.5 → 4.6 → next), these assumptions become stale overhead. Schedule regular stress tests: remove a scaffolding component and measure whether quality degrades. If not, the component was compensating for a limitation the model has outgrown. *(Anthropic)*

---

## Anti-Patterns (What Didn't Work)

### Designing the Ideal Harness Upfront

Building elaborate harness infrastructure before encountering real failures. Configuration should be reactive to observed problems, not speculative. *(HumanLayer)*

### Preemptive Tool Loading

Installing dozens of skills/MCP servers "just in case." Each idle tool adds context overhead and decision complexity without proven benefit. Start bare, add tools when specific failures justify them, pare down after stabilizing. *(HumanLayer)*

### Running Full Test Suites After Every Session

Running 5+ minute test suites after every agent session is wasteful. Use progressive tier targets for faster feedback during development; reserve full suites for commit gates. *(HumanLayer)*

### Micro-Optimizing Sub-Agent Tool Access

Finely tuning which tools each sub-agent can access creates "tool thrash" — constant adjustment overhead without proportional quality gains. Give sub-agents reasonable defaults and only restrict when problems emerge. *(HumanLayer)*
