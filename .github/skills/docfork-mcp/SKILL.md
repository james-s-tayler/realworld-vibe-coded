---
name: docfork-mcp
description: Use the Docfork MCP server’s get-library-docs tool to pull fresh, version-accurate documentation and code examples for external libraries and frameworks.
---

# Docfork MCP Skill – Fresh Library Docs on Demand

You have access to the `docfork` MCP server via the `get-library-docs` tool.

Docfork is specialized in fetching **up-to-date docs and code examples for software libraries** (frameworks, SDKs, UI kits, etc.). It pulls from the current online documentation so you don’t have to rely on stale model knowledge.

Use this skill any time you need **accurate, version-current documentation** while working in this repository.

---

## When to Use This Skill

Invoke `get-library-docs` whenever **library or framework knowledge is the bottleneck**, for example:

- The user asks:
    - “How do I configure X in Next.js/Tailwind/EF Core/etc.?”
    - “Show me how to do authentication/routing/forms with library Y.”
    - “What’s the recommended way to implement Z in this library?”
- You are about to introduce a **new library** into the codebase and need:
    - Setup instructions
    - Best-practice examples
    - Configuration nuances
- You suspect your own training data might be **out of date** (major version bumps, breaking changes, new APIs).
- You need **official-style examples** to compare against the patterns in this repo.

If the question is purely about **local project code** (internal classes, custom helpers, internal DSL), prefer reading the repository files directly first. Use Docfork when the answer depends on external library behavior.

---

## How to Call `get-library-docs`

> You don’t need to remember the exact JSON schema—your tools interface exposes that.
> Use this section as **semantic guidance** for how to populate arguments.

When calling `get-library-docs`:

1. **Pick the library identifier**
    - Prefer an **author/library pair** or well-known name, for example:
        - `vercel/next.js`
        - `tailwindlabs/tailwindcss`
        - `shadcn-ui/ui`
        - `vuejs/docs`
        - `nestjs/nest`
    - If the user only mentions a generic name (like “Next.js”), infer or refine to a canonical identifier when possible.

2. **Optionally specify a topic**
    - Use a short natural-language phrase describing the focus, e.g.:
        - `"routing and route groups"`
        - `"authentication with cookies"`
        - `"form validation with Zod"`
        - `"entity framework migrations and seeding"`
    - If the user has a very broad question (“teach me Next.js”), either:
        - Ask for a narrower topic, **or**
        - Start with a high-level topic like `"getting started"` or `"project setup"`.

3. **Optionally tune response size / detail**
    - If the tool exposes a token or size parameter, set it based on needs:
        - Use a **larger budget** for deep dives or multiple related examples.
        - Use a **smaller budget** for quick lookups to avoid overwhelming the context.

4. **Prefer one call per focused topic**
    - If multiple libraries are involved (e.g., `next.js` + `next-auth` + `prisma`), call `get-library-docs` separately for each library and topic when needed.
    - Reuse information you already fetched from previous tool calls in the same session instead of re-calling the tool unnecessarily.

---

## How to Use the Results in Your Answer

After calling `get-library-docs`, you will typically receive:

- A confirmation of which library was selected
- Relevant documentation sections
- Example code snippets
- Content focused on the requested topic (when provided)

**Your responsibilities:**

1. **Summarize & adapt, don’t just paste**
    - Extract the pieces that are relevant to the user’s request.
    - Rewrite or adapt the examples to match:
        - The project’s language/framework version
        - The existing conventions in this repo (folder structure, naming, patterns)
    - Avoid dumping long docs verbatim; instead, provide:
        - A concise explanation
        - 1–3 tailored examples
        - Any critical caveats or “gotchas”

2. **Align with this repository**
    - Cross-check the Docfork examples against the current code:
        - Imports and package versions
        - Existing patterns (e.g., how logging, DI, or routing is done here)
    - If Docfork shows multiple valid approaches, prefer the one most consistent with the existing code in this repository.

3. **Call out version or breaking-change details**
    - If the docs mention important changes between versions (e.g., React 17 vs 18, Next.js App Router vs Pages Router), explicitly surface those in your explanation.
    - When migrating or refactoring, clearly distinguish **old** vs **new** API shapes.

4. **Use Docfork for planning as well as implementation**
    - Before designing a new feature around a library, you can:
        - Fetch high-level docs about recommended architectures or patterns.
        - Use that to justify the approach you propose to the user.
    - Document any key references or links in the repo’s markdown (if that’s part of the workflow), so future humans and agents can see where decisions came from.

---

## Example Usage Patterns (for the Agent)

These are behavioral patterns to follow, not literal user-visible text.

### 1. Answering a focused “how do I do X with Y?” question

1. Identify that the user’s question is about an external library/framework.
2. Call `get-library-docs` with:
    - The appropriate library identifier
    - A topic describing the requested feature
3. Skim results, then:
    - Explain the relevant concepts in your own words.
    - Provide code tailored to this repo.
    - Mention any important configuration or setup steps.

### 2. Designing a new integration with a library

1. When the user asks to introduce or expand usage of a library:
    - Use `get-library-docs` to fetch best-practice docs for the relevant topic(s).
2. Propose an implementation strategy that follows those best practices.
3. Where helpful, reference the relevant doc sections in natural language (e.g., “According to the official routing guide…”), but keep the answer self-contained.

### 3. Validating existing code against current docs

1. When you suspect the code uses an outdated or deprecated pattern:
    - Call `get-library-docs` with the library and topic corresponding to the pattern.
2. Compare the docs with the existing code.
3. If there are differences:
    - Explain what changed.
    - Suggest an updated implementation.
    - Highlight risks (e.g., deprecated APIs, security implications).

---

## Fallbacks and Error Handling

If:

- `get-library-docs` fails,
- returns no useful results, or
- the library/topic appears unsupported,

then:

1. Say briefly that the Docfork lookup did not return usable docs.
2. Fall back to:
    - Your own existing knowledge about the library, and/or
    - General web or code reasoning tools available to you (if applicable in this environment).
3. Be explicit about any uncertainty and prefer conservative recommendations.

---

## Summary

- Use this skill whenever **external library documentation** is central to answering the question.
- Prefer `get-library-docs` over guessing APIs from memory, especially for:
    - New libraries,
    - Major version changes, and
    - Security-sensitive or configuration-heavy areas.
- Always adapt Docfork’s output to the **actual code and patterns** of this repository before proposing changes.
