---
name: roslyn-code-analysis
description: >
  Use the RoslynMCP MCP server's tools (ValidateFile and FindUsages) to validate
  and analyze C# files in this repository when making changes.
license: MIT
---

# Roslyn code analysis skill

This skill teaches you how to use the `RoslynMCP` MCP server to validate and
analyze C# code in this repository using Roslyn.

You have access to the `RoslynMCP` MCP server, which exposes the following tools:

- `ValidateFile`: validates a C# file using Roslyn and runs analyzers.
- `FindUsages`: finds all references to a symbol at a specific location in a file.

## When to use this skill

Use this skill whenever you:

- Implement or modify C# code in this repository.
- Are preparing a change for review or completion.
- Need to understand the impact of a change on usages of a symbol.

Whenever you finish implementing C# changes, you must run Roslyn validation on
the modified files before considering the task complete.

## Procedure: validate C# changes with RoslynMCP

When working on a task that involves C# (.cs) files in this repo, follow this loop:

1. **Identify changed C# files**
    - Look at the task description, diff, or PR context to determine which `.cs`
      files have been added or modified.

2. **Run `ValidateFile` for each changed file**
    - For each changed `.cs` file, call the `ValidateFile` tool from the
      `RoslynMCP` MCP server.
    - Use arguments consistent with the server's schema:
        - `filePath`: the repo-relative or absolute path to the C# file.
        - `runAnalyzers`: set this to `true` so Roslyn analyzers run as well.
    - Wait for the tool response and inspect the diagnostics it returns
      (syntax, semantic, and analyzer diagnostics).

3. **Handle diagnostics**
    - If `ValidateFile` reports any errors or warnings:
        - Summarize the issues for the user.
        - Propose or apply code fixes to address them.
        - After making fixes, **re-run `ValidateFile` on the same file** until:
            - Blocking errors are resolved, or
            - Remaining diagnostics are clearly explained as acceptable tradeoffs.

4. **Use `FindUsages` when needed**
    - If you change or remove a symbol (method, class, property, etc.) and need
      to understand the impact:
        - Call the `FindUsages` tool from the `RoslynMCP` server with:
            - `filePath`: the C# file where the symbol is defined or referenced.
            - `line` and `column`: the position of the symbol in that file.
    - Use the returned references to:
        - Update or fix all impacted call sites.
        - Confirm that no important usages are left broken.

5. **Only consider the task done after validation passes**
    - Before stating that a change is complete, ensure:
        - All changed `.cs` files have been validated with `ValidateFile`.
        - No unexpected diagnostics remain.
    - Clearly mention in your explanation that you ran RoslynMCP validation and
      summarize any remaining acceptable warnings.

## Notes

- This skill assumes the MCP server is registered under the name `RoslynMCP`.
- Prefer using RoslynMCP tools over ad-hoc grep-style searching when analyzing
  C# code, because RoslynMCP works with full project context and the Roslyn
  compiler platform.