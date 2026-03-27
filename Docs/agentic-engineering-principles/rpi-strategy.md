# Introducing the RPI Strategy

> Source: https://patrickarobinson.com/blog/introducing-rpi-strategy/

A Framework for Reliable AI-Driven Software Development

## Why Most AI Development Fails

Here's what typically happens: You ask an AI to build a feature. It generates 500 lines of code using libraries you don't have, inventing methods that don't exist, solving the wrong problem entirely.

The issue isn't the AI -- it's that we're asking it to read our minds. Without structure, even brilliant AI assistants become expensive random code generators.

## The RPI Strategy: Your AI Development GPS

The RPI Strategy turns your AI from an eager intern into a seasoned developer by breaking work into three focused phases -- Research, Plan, and Implement -- each with validation mechanisms that ensure you're ready to proceed.

The framework includes validation scales (FAR for Research, FACTS for Plan) that score your work to ensure it's factual, actionable, and properly scoped.

### The Three Phases

**Research: Build Context & Insight**

Start by researching the problem thoroughly -- gathering evidence, mapping code surfaces, validating findings. The FAR scale (Factual, Actionable, Relevant) ensures discoveries are based on facts, not assumptions. No more building on shaky foundations.

**Plan: Decide What to Do & How**

Break work into atomic tasks. Why atomic tasks? That keeps AI on track with simple instructions to check off as it goes. The FACTS scale validates that each task is Feasible, Atomic, Clear, Testable, and properly Scoped.

**Implement: Ship & Learn**

With validated context and properly sized tasks, AI executes each task systematically, with continuous validation through builds, tests, and lints. You stay in control of decisions while AI handles the implementation.

## A Real Example

Let's say you receive a new ticket: "Add ability for users to bulk delete their uploaded files."

**Research**: Instead of guessing requirements, your AI uses "Reverse Prompting" -- asking you clarifying questions one at a time. "Should this work from the file manager or dashboard?" "Any file type restrictions?" "What happens to shared files?" After the Q&A reveals insights you hadn't considered, the AI analyzes your codebase and documentation, then outputs a research markdown file. The FAR scale validates these findings are factual (based on actual code), actionable (you know exactly what to build), and relevant (solves the real user need).

**Plan**: Using the research doc in a fresh context, your AI analyzes requirements and creates a phased approach with atomic tasks with markdown checkboxes. Phase 1: "Add bulk selection UI to file manager." Phase 2: "Create delete confirmation modal with file count." Phase 3: "Implement backend bulk delete API." Each phase passes FACTS validation to ensure it's feasible and properly scoped.

**Implement**: With your plan doc loaded in fresh context, your AI iterates through tasks systematically. You choose your feedback loop: "Do a task, validate" for maximum control, "Do a phase, validate" for speed, or "Do the whole thing, validate" when you're confident. Quality gates -- tests, builds, lints -- must pass after each task. No hallucinations, no scope creep -- just focused execution that works.

## Why This Actually Works

The secret is in what we don't let AI do: make big decisions without validation. Instead, we use AI's strengths -- pattern matching, code generation, systematic execution -- while humans handle strategy and validation.

Those validation scales aren't just checkboxes. They're guardrails that prevent the most common AI failures:

* **Context overflow?** Eliminated by keeping tasks atomic (i.e. a command call, file edit)
* **Hallucination?** Prevented by FAR validation requiring factual evidence
* **Wrong problem solved?** Impossible when Research validates relevance first
* **Untestable code?** FACTS validation ensures plan has clear success criteria
