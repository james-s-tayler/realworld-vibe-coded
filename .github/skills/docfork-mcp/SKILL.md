---
name: docfork-mcp
description: Use the Docfork MCP server’s search and read tools to fetch fresh, version-accurate documentation and code examples for external libraries and frameworks.
---

# Docfork MCP Skill – Fresh Library Docs on Demand

You have access to the Docfork MCP server via two tools:

- **`docfork_search_docs`**
  Search for documentation across the web, GitHub, and other sources.
  **Arguments:**
    - `query` (required): A natural-language query including the library/framework
    - `tokens` (optional): Approximate token budget for the result

- **`docfork_read_docs`**
  Read the content of a documentation URL returned from search results.
  **Arguments:**
    - `url` (required)

Docfork is specialized in fetching **real-time, version-current documentation and examples** for external libraries. Use these tools whenever you need documentation that is more accurate than model memory.

---

## When to Use This Skill

Invoke Docfork tools whenever **external library/framework knowledge is the blocker**, for example:

- The user asks:
    - “How do I configure X in Next.js/Tailwind/EF Core/etc.?”
    - “What’s the recommended approach for Z in library Y?”
    - “How do I authenticate using NextAuth / FastAPI / Express middleware?”
- You detect:
    - The library may have changed versions
    - A breaking change may have occurred
    - The model might not have fresh or reliable knowledge
- You plan to introduce a **new external dependency** and need:
    - Setup instructions
    - Best practices
    - Example patterns

If the problem is purely about **this repository’s local code**, prefer reading internal files first.
Use Docfork for **external** knowledge.

---

## How to Use the Docfork Tools

### 1. Start with `docfork_search_docs`

This tool is used for *discovering the correct documentation URL(s)*.

**Your search query should combine:**
- The **library / framework name**
- The **topic** the user is asking about
- Additional context (language, version, config keywords)

**Examples:**
- `"Next.js routing app router dynamic segments"`
- `"TailwindCSS dark mode configuration"`
- `"Entity Framework Core migrations seeding best practices"`
- `"Shadcn UI components form validation"`
- `"Vue 3 composition API provide inject"`

**Token Budget Guidelines:**
- Use **higher tokens** (2000–4000) for deep or broad topics
- Use **lower tokens** (200–800) for quick fact lookups

### 2. Then call `docfork_read_docs`

Use this when:
- The search tool returns URLs
- You need the *actual content* of a doc page
- You want more detail or examples than the search summary provides

Feed it the specific URL you want read.

---

## Behavioral Guidance for the Agent

### 1. Summarize and Adapt the Docs

When you receive documentation text via `docfork_read_docs`:

- Do **not** paste large sections verbatim
- Extract only what solves the user’s specific problem
- Rewrite and adapt examples to match:
    - The project’s conventions
    - The version being used
    - The directory structure or style already in the repository

### 2. Align with This Repository’s Codebase

When Docfork provides examples:

- Compare the patterns with the existing repo
- Prefer approaches consistent with current project conventions
- Identify mismatches (naming, DI style, routing style, framework version)

### 3. Clarify Version-Specific Notes

If search results or docs mention:
- Deprecated APIs
- New recommended patterns
- Version migration notes
- Breaking changes

You should surface those explicitly in your answer.

### 4. Use Docfork for Architectural Planning

Before proposing a new feature using an external library, you may:

- Search for best practices
- Read high-level guides
- Base your design rationale on official patterns
- Include references in any design markdown if appropriate

---

## Example Usage Patterns (for the Agent)

### **Pattern 1 — “How do I do X with Y?”**

1. Detect external library usage
2. Call `docfork_search_docs` with a focused query
3. If needed, call `docfork_read_docs` on the most relevant result
4. Summarize and adapt the answer for the user and repository

---

### **Pattern 2 — Validating or Updating Existing Code**

1. Notice code using a possibly outdated pattern
2. Search for official docs on that feature
3. Read the most relevant URL
4. Compare the repo code with official patterns
5. Suggest updates or warn about deprecated APIs

---

### **Pattern 3 — Multi-Library Workflows**

If multiple libraries are involved (e.g., Next.js + Prisma + Zod):

- Perform **separate search/read cycles** per library
- Keep each call focused on one library + one topic
- Integrate and reconcile the results in your final answer

---

## Error Handling & Fallbacks

If:
- Docfork search fails
- No useful results are returned
- A URL cannot be read

Then:

1. State that Docfork did not provide usable results
2. Fall back on your own knowledge
3. Be explicit about uncertainty
4. Prefer conservative, stable recommendations

---

## Summary

- Docfork provides **fresh, authoritative documentation** via:
    - `docfork_search_docs`
    - `docfork_read_docs`
- Use these tools whenever the question depends on **external libraries**, especially for:
    - Setup
    - Configuration
    - Best practices
    - Migration and version changes
- Always adapt Docfork content to the repository’s conventions before responding.

