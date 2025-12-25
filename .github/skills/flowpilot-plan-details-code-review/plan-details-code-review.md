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
6. Verify idempotent script with `./build.sh DbMigrationsVerifyIdempotentScript`

### Test Infrastructure Phases
When updating test infrastructure:
- Consider parallelization opportunities (e.g., per-test org registration vs database wipe)
- Separate infrastructure prep from test addition
- Update helper methods before writing tests that use them

## Entity Design Conventions

### Organization Entity (Multi-tenancy)
Based on review feedback:
- **Name**: Default to `"New Company"` (hardcoded for simplicity)
- **Identifier**: Use `Guid` for uniqueness
- **Don't include**: Optional fields like `Description` unless explicitly needed

## Verification Requirements

### Every Phase Must Specify
1. **Exact Nuke targets to run** (not generic descriptions)
2. **Manual verification steps** when appropriate
3. **Expected log locations** for debugging
4. **Reality checks** during implementation

### Verification Section Template
```markdown
## Verification

Run the following Nuke targets to verify this phase:

./build.sh LintAllVerify
./build.sh BuildServer
./build.sh DbMigrationsVerifyAll
./build.sh TestServerPostmanAuth
./build.sh TestServerPostmanArticlesEmpty
./build.sh TestE2e
```

## Phase Dependencies and Ordering

### Test Strategy Evolution
Example from this migration:
- **Phase 10**: Update E2E test infrastructure (per-test org registration)
- **Phase 11**: Add E2E multi-tenancy tests (uses phase 10 infrastructure)
- **Phase 12**: Update slug uniqueness validation
- **Phase 13**: Add functional multi-tenancy tests
- **Phase 14**: Add logging enrichment

**Key insight**: Infrastructure prep phases should come **before** phases that depend on that infrastructure

### Cross-Phase References
When phases reference each other:
- Use explicit phase numbers: "Phase 10 (E2E test infrastructure prep)"
- Update all references when renumbering phases
- Keep phase-analysis.md in sync with detail files

## Common Mistakes to Avoid

1. **Assuming test setup**: Check if auth, environments, or configurations already exist
2. **Generic commands**: Always use specific build targets
3. **Wrong manual verification commands**: Use `RunLocalPublish` not `RunLocal`
4. **Missing idempotent script regeneration**: Always regenerate after creating migrations
5. **Incorrect log paths**: Check actual log structure (`Logs/{source}/...`)
6. **Frontend access assumptions**: Frontend is served through backend HTTPS, not direct vite
7. **Repeating completed work**: Don't re-verify frontend in backend phases
8. **Oversized phases**: Split phases that exceed 10-12 steps
9. **Missing cross-phase updates**: When renumbering, update all references

## Reality Testing Guidance

### During Phase Implementation
- Run individual Postman collection targets after each step
- Check logs in appropriate `Logs/` subdirectories
- Use `DbMigrationsVerifyAll` immediately after migration changes
- Inspect SQL queries in Serilog logs when verifying query filters
- Test with `RunLocalPublish` before marking phase complete

### When Stuck
- Check if EF SQL logging is enabled in Serilog configuration
- Review Audit.NET logs for transaction status (Committed vs RolledBack)
- Use correlation IDs to trace EntityFrameworkEvent and DatabaseTransactionEvent
- Run `.build.sh --help` to enumerate available targets

## Documentation Standards

### Phase Detail File Structure
Each phase must include:
1. **Phase Overview**: Brief description
2. **Prerequisites**: Explicit dependencies on prior phases
3. **Implementation Steps**: 5-10 detailed, actionable steps
4. **Known Risks & Mitigations**: Likelihood/impact ratings, fallback strategies
5. **Reality Testing During Phase**: Specific commands and checks
6. **Expected Working State After Phase**: Clear success criteria
7. **If Phase Fails**: Debug-first guidance, when to invoke `flowpilot stuck`
8. **Verification**: Exact Nuke targets and manual checks

### Writing Implementation Steps
Each step should:
- Specify **files affected**
- Include **expected outcome**
- Provide **reality check** command/method
- Be independently testable where possible

**Example**:
```markdown
**Step 2**: Add Organization entity to EF Core DbContext

- **File**: `App/Server/src/Server.Infrastructure/Data/AppDbContext.cs`
- **Action**: Add `DbSet<Organization> Organizations { get; set; }`
- **Reality check**: Compile with `./build.sh BuildServer`
```

## Summary Checklist for Phase Details

- [ ] All build targets are specific and verified to exist
- [ ] Log paths reflect actual repository structure
- [ ] Migration phases include idempotent script regeneration
- [ ] Test verification uses individual Postman collection targets
- [ ] Manual verification uses `RunLocalPublish`
- [ ] Phase scope is 5-10 steps (or explicitly justified if larger)
- [ ] Prerequisites explicitly list dependent phases
- [ ] Verification section lists exact commands to run
- [ ] No assumptions about test infrastructure - verified existing setup
- [ ] Frontend/backend separation respected (no cross-concern steps)
- [ ] Cross-phase references updated if phase numbers changed
