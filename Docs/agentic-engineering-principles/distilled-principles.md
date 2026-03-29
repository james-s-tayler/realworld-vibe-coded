# Distilled Principles of Agentic Engineering

Synthesized from 9 reference materials spanning Anthropic, Vercel, LangChain, HumanLayer, OpenAI, and independent practitioners. Principles are organized by order: 1st-order principles are directly stated across sources; 2nd-order principles emerge from combining insights across multiple sources; 3rd-order principles are the deeper structural truths about why these work.

---

## 1st-Order Principles (Explicit)

These are stated directly and repeatedly across the reference materials.

### 1. Context is the only lever

The contents of the context window are the **sole input** that determines output quality at inference time. You cannot change the model mid-session. You can only change what the model sees. Optimize context for correctness first, then completeness, then minimizing noise. Chroma's research confirms performance degrades at longer context lengths, especially when semantic similarity between questions and context is low. (ACE, Ralph, OpenAI, HumanLayer)

### 2. Research before planning, plan before implementing

Every high-performing workflow separates understanding from deciding from executing. Research builds a factual model of the problem space. Planning converts understanding into precise, verifiable steps. Implementation follows the plan mechanically. Skipping phases produces compounding errors downstream. (ACE, RPI, LangChain, Anthropic)

### 3. Validate at every phase boundary

Use explicit quality gates between phases. FAR (Factual, Actionable, Relevant) validates research. FACTS (Feasible, Atomic, Clear, Testable, Scoped) validates plans. Tests, lints, and builds validate implementation. Never proceed on unvalidated output. (RPI, LangChain, Ralph)

### 4. Separate generation from evaluation

Agents confidently praise their own work even when quality is mediocre. Independent evaluators, tuned for appropriate skepticism, are far more tractable than making a generator self-critical. This applies at every scale: dedicated evaluator agents, self-verification loops, and automated test suites all implement this separation. (Anthropic Harness Design, LangChain, Ralph)

### 5. Work incrementally, one feature at a time

Feature-by-feature progress prevents context exhaustion mid-implementation and reduces debugging surface area. Each increment should leave the codebase in a clean, committable state. Incomplete work is worse than no work. (Anthropic Long-Running Agents, ACE, Ralph)

### 6. Protect the parent context

Use subagents for searching, summarizing, and exploring so the parent agent's context window remains clean for implementation. Subagents are about context control, not role-playing — personality-based agents (frontend/backend/analyst) don't work; task-based encapsulation does. Subagents act as "context firewalls" with fresh, small, high-relevance context windows. The ideal subagent output is a focused markdown summary with key findings and source citations (filepath:line). (ACE, Ralph, HumanLayer)

### 7. Encode knowledge as discoverable artifacts in the codebase

Agent knowledge equals what the agent can see. Unseen knowledge does not exist to the agent. Structure specifications, architecture decisions, progress state, and conventions as markdown files within the repository. Progressive disclosure lets agents start small and explore deeper as needed. (OpenAI, ACE, Anthropic Long-Running Agents)

### 8. Simplify the tool surface

Removing 80% of tools yielded 3.5x faster execution and 100% success rate. Models reason better with fewer, more general tools than with many specialized ones. More tools means more decision points means more opportunities for wrong choices. If a tool duplicates CLI functionality already in training data, prompt the agent to use the CLI instead. Replace MCP servers with custom CLIs where possible to save thousands of tokens per invocation. (Vercel, HumanLayer)

### 9. Backpressure through automated verification

Tests, type checks, linters, and security scans create selection pressure that rejects invalid generations. This is not optional quality assurance -- it is the mechanism by which quality emerges. Without backpressure, agents produce plausible-looking slop. **Critical: verification must be context-efficient.** Running full test suites (4,000+ lines) floods context and causes hallucination. Swallow successful output; surface only errors. (Ralph, LangChain, RPI, HumanLayer)

### 10. Human review at highest-leverage points

A bad line of research leads to thousands of bad lines of code. A bad line of a plan leads to hundreds. A bad line of code is just a bad line of code. Focus human attention on the earliest, highest-leverage artifacts: research findings and implementation plans. Code review matters least. (ACE, OpenAI)

---

## 2nd-Order Principles (Emergent)

These are not stated in any single source but emerge from the patterns across all of them.

### 11. The compaction-expansion rhythm

Every effective workflow follows the same cycle: **expand** (research, explore, generate) then **compact** (distill findings into a structured artifact). Research expands understanding; a research document compacts it. Planning expands options; a plan compacts decisions. Implementation expands code; a commit compacts state. This rhythm is the heartbeat of agent-driven development. The critical skill is knowing when to switch modes.

### 12. Session boundaries are features, not bugs

Ralph reloads PROMPT.md every loop. Anthropic's long-running agents read progress files at session start. ACE advocates keeping context utilization at 40-60% and starting fresh frequently. The constraint of finite context windows forces compaction, which forces clarity, which improves outcomes. Unlimited context would actually make agents worse by allowing accumulated noise. Each session boundary is a natural compaction point that resets noise to zero.

### 13. The tool paradox resolves through foundation quality

Vercel removed tools and improved. LangChain added middleware and improved. These aren't contradictions. The right abstraction level depends on your foundation: strong foundations (well-structured, consistently named, thoroughly documented) need fewer tools because the agent can navigate directly. Weak foundations need more scaffolding to compensate. The correct response to poor agent performance is usually to improve the foundation, not add more tools.

### 14. Generator-evaluator separation is fractal

Anthropic applies it at the agent level (separate evaluator agents). It also appears at the workflow level: research validates the problem model, the plan validates the approach, tests validate the implementation. At every scale, the pattern of "produce something, then have an independent process judge it" creates quality. The number of evaluation layers should match the complexity and blast radius of the task.

### 15. Specs replace code as the durable artifact

ACE argues specs are the new source code. OpenAI structures everything as markdown in a docs/ directory. RPI produces research documents and phased plans. When AI writes 99% of code, the specifications that drive generation become the real intellectual property. Code becomes a compiled artifact -- important, but derived and regenerable. The human's job shifts from writing code to writing precise specifications.

### 16. Evolve the harness from failures, not from theory

Don't design the ideal harness upfront. Don't install dozens of skills/MCP servers "just in case." Start simple. Add configuration only when specific failures occur. Test and iterate. Discard what doesn't work. Carefully pare down capabilities after identifying actual needs. Installing tools preemptively creates context bloat and decision overhead that actively hurts performance. (HumanLayer)

### 17. Harness assumptions encode model limitations

Every component in a harness encodes an assumption about what the model cannot do on its own. As models improve from 4.5 to 4.6 and beyond, scaffolding that compensated for earlier weaknesses becomes unnecessary overhead. The harness must be treated as a living hypothesis, not fixed architecture. Regular stress-testing of assumptions prevents accumulated cruft that actively hinders better models. Models post-trained on specific harnesses (Claude on Claude Code, GPT-5 Codex on Codex) may perform better in their trained-on harnesses, but can also over-fit — Terminal Bench 2.0 shows Opus 4.6 at #33 in Claude Code but #5 in a different harness. (HumanLayer)

### 18. Errors compound downstream, corrections compound upstream

This is the information-theoretic reason why Research > Plan > Implement ordering matters. A misunderstanding in research propagates through the plan into potentially thousands of lines of wrong code. Conversely, improving research quality has multiplicative effects on everything downstream. The leverage of each phase is proportional to how many downstream phases depend on it.

### 19. Mental alignment is the hidden product of process

ACE's most important insight isn't about productivity -- it's about teams losing touch with their own codebase when AI writes most of the code. The real value of research/plan/implement is that the artifacts (research docs, plans) keep humans aligned on what the system does and why. Without this, velocity becomes chaos. A team shipping fast but misaligned is worse than a team shipping slowly in agreement.

---

## 3rd-Order Principles (Structural)

These are the deep truths about the nature of the problem itself.

### 20. Agent engineering is control engineering, not intelligence engineering

The consistent finding across every source is that architecture around the model matters more than the model itself. LangChain went from Top 30 to Top 5 with the same model by changing only the harness. Vercel achieved 100% success by simplifying architecture. This means the discipline of agent engineering is fundamentally about designing feedback loops, error correction mechanisms, and information flow -- the same concerns as control systems engineering. The model is the actuator. The harness is the control system.

### 21. Quality emerges from selection pressure, not single-shot generation

Ralph's loop + backpressure. LangChain's plan-build-verify-fix cycle. RPI's validation gates. Anthropic's generator-evaluator. All create the same dynamic: generate, test, reject, regenerate. No source relies on the agent getting it right the first time. Quality is an emergent property of iterated generation under selection pressure. This is Darwinian, not intelligent design. The engineering challenge is designing the right selection pressures (tests, evaluators, human review), not making generation perfect.

### 22. The information-theoretic ceiling on guidance

OpenAI discovered that too much guidance becomes non-guidance -- agents pattern-match locally and contradictions pile up. There is a fixed budget of "bits of useful guidance" you can provide before noise overwhelms signal. The optimal point is the **minimum sufficient structure**: just enough constraint to direct behavior without overwhelming the model's ability to reason. This is why monolithic instruction files become "graveyards of stale rules" and why progressive disclosure (small entry point, deeper docs available on demand) works better. ETH Zurich found LLM-generated agentfiles actually hurt performance while costing 20%+ more, and codebase overviews/directory listings provided no benefit — confirming that more guidance is not always better guidance. (OpenAI, HumanLayer)

### 23. Compaction is lossy compression optimized for action

When research is distilled into a plan, information is discarded. When a session's work is summarized into a progress file, nuance is lost. This is not a deficiency -- it is the mechanism. The art of compaction is choosing what to preserve and what to discard. This is why human review at compaction boundaries (research-to-plan, plan-to-implementation) is the highest-leverage intervention: humans are serving as the compression algorithm, deciding which information survives into the next phase.

### 24. The legibility imperative inverts traditional software engineering

"Agent knowledge equals what the agent can see" means your codebase must be legible to agents, not just humans. This inverts decades of software engineering priorities. Code that is "obvious to any experienced developer" may be opaque to an agent without explicit documentation. Naming conventions, directory structure, README files, and architecture documents are no longer nice-to-haves -- they are the interface through which agents understand your system. You now write for two audiences, and the overlap is significant but not complete.

### 25. Diminishing returns define the human-agent boundary

Ralph works for greenfield at ~90% completion. Anthropic's full harness costs 22x more than solo agent for higher quality. The pattern is consistent: agent engineering has massive returns up to a point, then human expertise becomes irreplaceable. The last 10% -- subtle race conditions, deep architectural insight, novel problem-solving in unfamiliar domains -- remains disproportionately expensive for agents. The practical skill is recognizing where you are on this curve and switching strategies accordingly.

### 26. The meta-principle: design systems that become simpler over time

Every harness assumption is a bet against the model. Better models invalidate old assumptions. The best agent engineering practice is therefore to build systems where removing complexity is easy and expected. Hard-coded workarounds for model limitations should be isolated and labeled. Scaffolding should be designed for removal. The goal is not the most sophisticated harness -- it is the minimum viable harness for today's model, with clear paths to simplification tomorrow.
