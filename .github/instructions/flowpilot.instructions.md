---
applyTo: ".flowpilot/flows/**"
---

# Flowpilot guidelines

Flowpilot is an orchestration tool to help vibe-coders and agentic coding assitants like Github Copilot collaborate on
creating and executing complex, multi-stage featuring development, refactorings and migrations
via a stateful workflow persisted to the repository as a set of markdown files with a given schema,
tracked by a checklist in state.md and orchestrated by the flowpilot cli.

## Flowpilot CLI

- `flowpilot init $plan_name`
  - the developer will run this, commit it to the repo, and then open an issue asking the agent to run `flowpilot next $plan_name`.
- `flowpilot next $plan_name`
  - this will interrogate state.md and copy the relevant markdown file from .flowpilot/template to relevant place under .flowpilot/$plan_name
  - it will then print a message to the console telling you which file it output and giving you specific instructions on how to update it
  - you must treat its output as your next prompt and execute its instructions.
- `flowpilot lint $plan_name`
  - this enforces the following rules:
    - state.md checklist is checked off in order
    - state.md checklist is checked off one item per-commit
    - state.md checklist item being checked off also has expected accompanying files and those files are valid
  - git hooks will be configured to run this on commit
  - CI will be configured to run this before merging

## .flowpilot/ Folder Structure

Plans are organized under the `.flowpilot` directory with the following structure:

```
.flowpilot/
└── template/
|       ├── state.md
|       ├── references.md
|       ├── system-analysis.md
|       ├── key-decisions.md
|       ├── phase-analysis.md
└── flows/
    └── $plan_name/
            └── meta/
            |   ├── state.md
            |   ├── references.md
            |   ├── system-analysis.md
            |   ├── key-decisions.md
            |   ├── phase-analysis.md
            └── plan/
                ├── phase_1_$title.md
                ├── phase_2_$title.md
                └── phase_n_$title.md
```

* **`$plan_name`**: A descriptive name for the migration (e.g., `entity-framework-upgrade`, `api-versioning-migration`)
* **`state.md`**: A state-machine implemented as a checklist that tracks and orchestrates the plan through both creation and execution.
* **`references.md`**: Documents all research sources and references that informed the migration plan
* **`system-analysis.md`**: An analysis of the parts of the current system relevant to the migration plan.
* **`key-decisions.md`**: An outline of key decision points that must be decided before moving to phase analysis.
* **`phase-analysis.md`**: An outline of the high level goals of each phase that must be decided on before detailing each phase.
* **`phase_n_$title.md`**: Details of each phase in its own file, numbered sequentially with a descriptive title

## References.md

**Before starting to write the migration plan**, Copilot must conduct thorough research:

1. **Use the mslearn MCP server** to search Microsoft documentation for relevant patterns, best practices, and implementation guidance
2. **Perform web searches** to gather additional context, community practices, and potential pitfalls
3. **Document all findings** in `Docs/$migration_plan/references.md` with:
   - Source title and URL
   - Key takeaways
   - Relevance to the migration plan

### Example references.md Format

```markdown
# References for [Migration Name]

## Microsoft Learn Documentation

### [Article Title](URL)
**Key Takeaways:**
- Point 1
- Point 2

**Relevance:** How this informs our migration strategy

## Web Resources

### [Article/Blog Title](URL)
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
