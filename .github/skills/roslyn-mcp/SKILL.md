---
name: roslyn-mcp
description: Use the RoslynMCP server to search, navigate, and analyze C# solutions with compiler-accurate results.
---

# Roslyn MCP Skill – Deep C# Code Intelligence

You have access to the **Roslyn MCP server**, a C#-focused MCP service that uses the **Roslyn compiler platform** to provide rich analysis and navigation for C# codebases.

Use this skill any time you need **accurate, solution-wide understanding of C# code**, beyond what simple text search or local heuristics can provide.

---

## What This Skill Is Good At

Roslyn MCP works at the **solution** level (`.sln`) and understands **symbols**, not just text.

It can:

- 🔎 **Search symbols by wildcard**
    - Find classes/methods/properties by patterns like `*Service`, `Get*User`, `I*Repository`, etc.
- 🧭 **Find references**
    - Locate all usages of a class, interface, method, property, etc. across the whole solution.
- 🔬 **Inspect symbol information**
    - Get signatures, declarations, containing types/namespaces, and other metadata about a symbol.
- 🕸 **Analyze dependencies**
    - Explore project-to-project dependencies and namespace usage patterns.
- 📈 **Analyze code complexity**
    - Identify high-cyclomatic-complexity methods to target for refactoring.

Under the hood it uses **multi-level caching** and **incremental analysis**, so it scales to large solutions.

---

## When to Use Roslyn MCP (vs Normal Copilot Help)

Use this skill when:

- You need **“where is this used?”** across the solution.
- You want to find **all classes/methods matching a naming pattern**.
- You’re trying to understand **how projects depend on each other**.
- You want to **hunt for complex methods** to refactor (e.g., high cyclomatic complexity).
- You’re trying to build a mental model of a **large or unfamiliar C# solution**.

Don’t use it when:

- You only need help with a **single file’s code** (Copilot can usually infer from context).
- You’re asking generic C# or .NET questions that don’t require analyzing the actual repo.

---

## How to Ask (Prompt Patterns)

Always mention:

1. **The solution path** (absolute or clearly relative)
2. **What you’re looking for** (specific symbol / pattern / threshold)

Examples (adapt these to your repo paths):

### 1. Search for Symbols

Ask to find symbols by **wildcard pattern**:

> Search for all classes ending with `Service` in my solution at `C:\MyProject\MyProject.sln`.

> Search for methods starting with `Get` on `User*` types in `C:\MyProject\MyProject.sln`.

> In `./src/MyApp.sln`, list all interfaces matching `I*Repository`.

This uses the **`SearchSymbols`** tool.

---

### 2. Find References

Ask to locate **usages** of a symbol:

> Find all references to the `UserRepository` class in `C:\MyProject\MyProject.sln`.

> In `./MySolution.sln`, show where `OrderService.ProcessOrder` is used.

This uses the **`FindReferences`** tool.

---

### 3. Get Symbol Information

Ask for details about a specific symbol:

> Get information about the `CalculateTotal` method in `C:\MyProject\MyProject.sln`.

> In `./src/Storefront.sln`, show full info for the `CartItem` class (namespace, containing assembly, properties, etc.).

This uses the **`GetSymbolInfo`** tool.

---

### 4. Analyze Dependencies

Ask about **project and namespace dependencies**:

> Analyze dependencies for the solution at `C:\MyProject\MyProject.sln`.

> For `./EnterpriseApp.sln`, show project dependency relationships and any circular dependencies.

This uses the **`AnalyzeDependencies`** tool.

---

### 5. Code Complexity Analysis

Ask to find **complex methods**:

> Find methods with complexity higher than 7 in `C:\MyProject\MyProject.sln`.

> In `./src/LegacySystem.sln`, list methods with very high cyclomatic complexity (over 15) and tell me which files they’re in.

This uses the **`AnalyzeCodeComplexity`** tool.

---

## Tool Reference (for Copilot’s Internal Use)

Roslyn MCP exposes the following tools:

1. **`SearchSymbols`**
    - Input: solution path, wildcard pattern, optional symbol kinds (class, method, property, etc.).
    - Use for: pattern-based symbol discovery.

2. **`FindReferences`**
    - Input: solution path, fully-qualified symbol identifier or enough info to resolve it.
    - Use for: “where is this used?” queries across the solution.

3. **`GetSymbolInfo`**
    - Input: solution path, symbol name/identifier.
    - Use for: detailed info on types, methods, properties, etc.

4. **`AnalyzeDependencies`**
    - Input: solution path.
    - Use for: project graph, dependency chains, namespace usage.

5. **`AnalyzeCodeComplexity`**
    - Input: solution path, complexity threshold.
    - Use for: finding high-complexity methods, refactoring targets.

> As Copilot, prefer these tools whenever the user is asking about **solution-wide C# structure, relationships, or complexity**, rather than trying to infer from local context alone.

---

## Prompting Tips & Best Practices

For **better results**, try to:

- **Be precise about the solution path**
    - e.g., `./src/MyApp.sln` instead of just “my solution”.
- **Name the symbol clearly**
    - Class: `UserRepository`
    - Method: `OrderService.ProcessOrder` or `ProcessOrder` on `OrderService`
- **Specify thresholds**
    - “complexity higher than 7” instead of just “complex methods”.
- **Ask for structured output**
    - “Include file paths and line numbers if available.”
    - “Summarize the top 10 most complex methods.”

---

## Example Combined Workflows

You (the user) can chain tasks via Copilot, e.g.:

> In `./Backend.sln`, find methods with complexity higher than 10. Then, for the top 5 most complex methods, show me their file paths and a short explanation of why they’re complex (e.g. deep nesting, many branches, etc.), and suggest specific refactoring ideas.

> In `./Monolith.sln`, analyze dependencies and list any projects that depend on `Core.Domain`. Then, for each of those projects, show one example of where `Order` or `Customer` types are used.

Roslyn MCP will provide the **ground-truth data**, and Copilot can then **summarize, prioritize, and suggest refactors** based on that data.

---
