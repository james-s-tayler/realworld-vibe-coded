---
applyTo: ".flowpilot/plans/**/phase-*-details.md"
---

## phase-n-details.md

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

./build.sh LintAllVerify
./build.sh BuildServer
./build.sh TestServer
./build.sh TestE2e
```

Ensure all targets pass before proceeding to the next phase.

**To enumerate available targets**, use:
```bash
./build.sh --help
```

Select relevant build and test targets based on the areas affected by the phase.

### Prohibited Content

The following items **must never** be included in any phase:

❌ **Completion checklists**: Do not add `- [ ]` checkbox lists tracking completion status

❌ **Documentation updates**: Do not include steps to update README files, API docs, or similar documentation

❌ **Timelines or estimates**: Do not include time estimates, deadlines, or effort predictions

## Best Practices

1. **Keep phases focused**: Each phase should accomplish a cohesive set of changes
2. **Maintain working state**: Every phase should leave the codebase in a functional, testable state
3. **Explicit verification**: Never assume verification; always list specific targets to run
4. **Clear prerequisites**: Make dependencies between phases explicit
5. **Actionable steps**: Write steps that can be directly executed without interpretation
6. **Research-driven**: Base plans on documented best practices and official guidance
