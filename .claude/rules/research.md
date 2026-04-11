## Research-First Planning

Before proposing any plan involving a framework or library, verify assumptions against actual documentation.

### Required Pre-Planning Steps

1. **Search indexed docs first.** Use `mcp__docs-mcp-server__search_docs` to verify:
   - API surface and method signatures
   - Token/variable availability (CSS custom properties vs SCSS-only, etc.)
   - Import paths and package exports
   - Runtime behavior and configuration options

2. **Index missing libraries.** If the library isn't found:
   - Run `mcp__docs-mcp-server__list_libraries` to check what's indexed
   - Run `mcp__docs-mcp-server__scrape_docs` to index it before proceeding

3. **Cite sources in plans.** Include doc findings with source URLs — never rely solely on training data for framework-specific details. Training data goes stale; docs don't.

### High-Risk Areas (Always Verify)

- **Design tokens** — whether tokens exist as CSS custom properties, SCSS variables, JS constants, or not at all
- **Build tool configuration** — plugin APIs, config schema, supported options
- **API surface and imports** — what a package actually exports vs what you assume it exports
- **Version-specific behavior** — breaking changes between major versions

### Why This Matters

Incorrect assumptions compound silently. A library might expose tokens only as build-time variables, not runtime custom properties — or a config option might not exist in the version you're using. Every plan built on unchecked assumptions risks implementing something that silently doesn't work. A single docs search catches this before any code is written.
