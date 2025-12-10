---
applyTo: "Docs/**"
---

# Migration Plan Documentation Guidelines

This document defines the guidelines for creating and executing complex, multi-stage refactorings and migration plans in this repository.

## File Structure

Migration plans must be organized under the `Docs/` directory with the following structure:

```
Docs/
└── $migration_plan/
    ├── references.md
    ├── phase_1_$title.md
    ├── phase_2_$title.md
    └── phase_n_$title.md
```

* **`$migration_plan`**: A descriptive name for the migration (e.g., `entity-framework-upgrade`, `api-versioning-migration`)
* **`references.md`**: Documents all research sources and references that informed the migration plan
* **`phase_n_$title.md`**: Each phase in its own file, numbered sequentially with a descriptive title

## Research Phase

**Before starting to write the migration plan**, Copilot must conduct thorough research:

1. **Use the mslearn MCP server** to search Microsoft documentation for relevant patterns, best practices, and implementation guidance
2. **Perform web searches** to gather additional context, community practices, and potential pitfalls
3. **Document all findings** in `Docs/$migration_plan/references.md` with:
   - Source title and URL
   - Key takeaways
   - Relevance to the migration plan
   - Date accessed

### Example references.md Format

```markdown
# References for [Migration Name]

## Microsoft Learn Documentation

### [Article Title](URL)
**Accessed:** YYYY-MM-DD
**Key Takeaways:**
- Point 1
- Point 2

**Relevance:** How this informs our migration strategy

## Web Resources

### [Article/Blog Title](URL)
**Accessed:** YYYY-MM-DD
**Key Takeaways:**
- Point 1
- Point 2

**Relevance:** How this applies to our codebase
```

## Phase Structure

Each phase file must follow this structure:

### Required Sections

1. **Phase Overview**: Brief description of what this phase accomplishes
2. **Prerequisites**: What must be completed before starting this phase
3. **Implementation Steps**: Detailed, actionable steps to complete the phase
4. **Verification**: Explicit list of Nuke targets to run to verify success

### Verification Targets

Each phase **must** end with an explicit "Verification" section listing the Nuke targets to run:

```markdown
## Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestE2e
```

Ensure all targets pass before proceeding to the next phase.
```

**To enumerate available targets**, use:
```bash
./build.sh --help
```

Select relevant build and test targets based on the areas affected by the phase:
- **Linting**: `LintServerVerify`, `LintClientVerify`, `LintNukeVerify`, `LintAllVerify`
- **Building**: `BuildServer`, `BuildClient`, `BuildServerPublish`
- **Testing**: `TestServer`, `TestClient`, `TestServerPostman`, `TestE2e`
- **Database**: `DbMigrationsVerifyAll`, `DbMigrationsVerifyApply`, `DbMigrationsVerifyIdempotentScript`

### Prohibited Content

The following items **must never** be included in any phase:

❌ **Completion checklists**: Do not add `- [ ]` checkbox lists tracking completion status

❌ **Documentation updates**: Do not include steps to update README files, API docs, or similar documentation

❌ **Timelines or estimates**: Do not include time estimates, deadlines, or effort predictions

### Example Phase Structure

```markdown
# Phase 1: Preparation and Setup

## Phase Overview

This phase prepares the codebase for the migration by establishing the necessary infrastructure and tooling.

## Prerequisites

- Clean main branch with all tests passing
- All dependencies up to date

## Implementation Steps

### Step 1: Install Required Packages

Update `App/Server/Server.csproj` to include:

```xml
<PackageReference Include="PackageName" Version="X.Y.Z" />
```

### Step 2: Create Infrastructure

Create the following new files:

1. `App/Server/Infrastructure/NewComponent.cs`
   - Implement base functionality
   - Add appropriate interfaces

2. `App/Server/tests/Unit/NewComponentTests.cs`
   - Add unit tests covering core scenarios

### Step 3: Update Configuration

Modify `App/Server/Program.cs` to register new components.

## Verification

Run the following Nuke targets to verify this phase:

```bash
./build.sh LintServerVerify
./build.sh BuildServer
./build.sh TestServer
```

Ensure all targets pass before proceeding to Phase 2.
```

## Decision Points

When a phase presents **major choices between different options**, Copilot must:

1. **Stop before writing subsequent phases**
2. **Document each option** with:
   - Description of the approach
   - Pros and cons
   - Impact on the codebase
   - Recommended option (if any)
3. **Request clarification from the user** about which option to pursue
4. **Wait for user response** before continuing with subsequent phases

### Example Decision Point

```markdown
# Phase 2: Choose Migration Strategy

## Decision Required

There are two viable approaches for this migration:

### Option A: Incremental Migration

**Description:** Migrate components one at a time while maintaining backward compatibility.

**Pros:**
- Lower risk
- Can be done gradually
- Easier to roll back

**Cons:**
- Requires maintaining parallel implementations temporarily
- Takes longer to complete
- More code churn overall

**Impact:** Requires additional abstraction layers during transition period.

### Option B: Big Bang Migration

**Description:** Migrate all components in a single coordinated effort.

**Pros:**
- Faster completion
- Cleaner final state
- Less temporary code

**Cons:**
- Higher risk
- Larger testing surface
- Harder to roll back

**Impact:** Requires comprehensive testing before deployment.

**Recommendation:** Option A is recommended due to lower risk profile.

---

**ACTION REQUIRED:** Please confirm which option to pursue before I continue writing Phase 3 and beyond.
```

## Best Practices

1. **Keep phases focused**: Each phase should accomplish a cohesive set of changes
2. **Maintain working state**: Every phase should leave the codebase in a functional, testable state
3. **Explicit verification**: Never assume verification; always list specific targets to run
4. **Clear prerequisites**: Make dependencies between phases explicit
5. **Actionable steps**: Write steps that can be directly executed without interpretation
6. **Research-driven**: Base plans on documented best practices and official guidance

## When Copilot Writes Migration Plans

1. **Start with research**: Use mslearn and web search before writing any phases
2. **Document sources**: Record all references in `references.md`
3. **Structure properly**: Follow the file and phase structure exactly
4. **Verify requirements**: Ensure each phase ends with verification targets
5. **Stop at decisions**: Don't proceed past major decision points without user input
6. **Stay focused**: Don't include checklists, documentation, or timelines

---

**Scope:** Migration planning only. This file guides the creation of migration plans, not the execution of the migrations themselves.
