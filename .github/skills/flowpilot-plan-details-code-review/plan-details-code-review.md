# FlowPilot Plan Details Code Review Guidelines

This document captures rules, heuristics, and best practices for reviewing and writing phase detail files in FlowPilot migration plans, derived from code review feedback on the Finbuckle.MultiTenant migration plan.

## General Principles

### 1. Accuracy Over Assumption
- **Always** verify actual build targets, paths, and tooling against the repository structure
- **Never** assume generic commands exist without checking for specific variants
- **Example violations**:
  - Using `./build.sh TestServerPostman` when individual collection targets exist
  - Using `./build.sh RunLocal` instead of `./build.sh RunLocalPublish`
  - Assuming `dotnet ef database update` when `./build.sh DbMigrationsVerifyAll` is the standard

### 2. Phase Scope and Size
- Target **5-10 implementation steps** per phase (Small phases)
- Avoid phases with **20+ steps** (Large phases that should be split)
- Each phase must leave the system in a **working, testable state**
- Phases should be independently committable with full test validation

### 3. Test Infrastructure Awareness
- Understand existing test configurations before suggesting changes
- Leverage existing test infrastructure (e.g., Postman bearer token auth is already configured)
- Don't suggest adding test infrastructure that already exists

## Build and Test Targets

### Migration Verification
- **Migrations run automatically on app startup** - don't suggest manual `dotnet ef database update`
- **Always regenerate idempotent.sql** after creating migrations: `dotnet ef migrations script --idempotent`
- **Verification targets**:
  - `./build.sh DbMigrationsVerifyAll` - verifies migrations apply successfully
  - `./build.sh DbMigrationsVerifyIdempotentScript` - verifies idempotent script is up to date

### Postman Test Targets
Individual collection targets exist for each Postman collection:
- `./build.sh TestServerPostmanArticlesEmpty`
- `./build.sh TestServerPostmanAuth`
- `./build.sh TestServerPostmanProfiles`
- `./build.sh TestServerPostmanFeedAndArticles`
- `./build.sh TestServerPostmanArticle`

**Never use**: Generic `./build.sh TestServerPostman` - always specify individual collections

### Manual Verification
- **Correct**: `./build.sh RunLocalPublish`
- **Incorrect**: `./build.sh RunLocal`

## File Paths and Structure

### Log Locations
Logs are organized by executable source:
- `Logs/Test/e2e/` - E2E test logs
- `Logs/Test/Postman/` - Postman test logs
- `Logs/RunLocal/` - Local dev server logs
- Serilog logs: `Logs/Test/Postman/Server.Web/Serilog/`
- Audit logs: `Logs/Test/Postman/Server.Web/Audit.NET/`

### POC and Temporary Code
- **Use repository structure**: `Task/PoC/{ProjectName}` instead of `/tmp/`
- Keep POC code in the repository for reference and reproducibility

### Frontend Access in Development
- Frontend is accessed through **backend HTTPS URL** (not direct vite dev server)
- App/Server serves static assets, SPA proxy forwards to vite dev server
- **Incorrect**: Direct access to `localhost:5173`

## Phase-Specific Patterns

### Pure Refactoring Phases
When a phase is a pure refactoring:
- **No URL, behavior, or payload changes**
- **Existing tests should work without modification**
- Focus verification on ensuring no behavior change occurred
- Update configuration/wiring, not test expectations

**Example**: Phase 6 - Creating custom FastEndpoints for Identity
- Tests don't need updates (same URLs, same payloads)
- Only Identity endpoint mapping configuration needs changes

### Frontend vs Backend Phases
- **Frontend phase**: Only frontend changes, no backend verification needed
- **Backend phase**: Only backend changes, no frontend verification needed
- **Don't repeat work**: If frontend was completed in phase 2, don't re-verify in phase 3

### Migration Phases
Pattern for phases that add/modify database entities:
1. Create/modify entity classes
2. Generate EF Core migration
3. **Regenerate idempotent.sql**: `dotnet ef migrations script --idempotent`
4. Update Audit.NET configuration if needed
5. Verify with `./build.sh DbMigrationsVerifyAll`

### Test Infrastructure Phases
When updating test infrastructure:
- Consider parallelization opportunities (e.g., per-test org registration vs database wipe)
- Separate infrastructure prep from test addition
- Update helper methods before writing tests that use them
