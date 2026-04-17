---
description: Create a structured implementation plan from interview findings
argument-hint: (no arguments - reads from workflow artifacts)
---

# Create Implementation Plan

**Workflow ID**: $WORKFLOW_ID

---

## Phase 1: LOAD

Read the interview findings:
- Read `$ARTIFACTS_DIR/interview.md` for the feature requirements and scope

Read the project context:
- Read `CLAUDE.md` if it exists — understand ALL project conventions and constraints
- Read any rules files referenced by CLAUDE.md
- Identify the project's tech stack, build system, and test infrastructure

### PHASE_1_CHECKPOINT
- [ ] Interview findings loaded
- [ ] Project conventions understood
- [ ] Tech stack identified

## Phase 2: EXPLORE

Explore the codebase to understand what exists:
- Identify the areas of code that will be affected
- Read existing implementations of similar features for patterns to follow
- Understand the dependency graph (e.g., backend must be built before frontend clients can use new types)
- Check for any generated code that needs regeneration after changes
- Identify test infrastructure and what kinds of tests exist

### PHASE_2_CHECKPOINT
- [ ] Affected code areas identified
- [ ] Existing patterns documented
- [ ] Dependency ordering understood
- [ ] Test strategy identified

## Phase 3: PLAN

Create a structured implementation plan with these properties:

### Task Ordering
- Respect dependency ordering (e.g., API changes before client code, schema before business logic)
- Group related changes that should be committed together
- Place generated code regeneration steps at the correct points in the sequence
- Each task should leave the codebase in a compilable state

### Task Definition
For each task, include:
- **Title**: Short, descriptive
- **Description**: What to implement and why
- **Files**: Which files to create or modify
- **Acceptance criteria**: How to verify the task is complete
- **Build check**: What command to run after to verify compilability

### Plan Structure
```markdown
# Implementation Plan

## Overview
[1-2 sentence summary]

## Tasks

### Task 1: [Title]
**Description**: ...
**Files**: ...
**Acceptance criteria**: ...
**Build check**: ...

### Task 2: [Title]
...
```

### PHASE_3_CHECKPOINT
- [ ] All tasks defined with clear acceptance criteria
- [ ] Tasks are ordered respecting dependencies
- [ ] Each task leaves code in compilable state
- [ ] No gaps — plan covers the full scope from interview

## Phase 4: WRITE

Write the plan to `$ARTIFACTS_DIR/plan.md`.

### PHASE_4_CHECKPOINT
- [ ] Plan written to `$ARTIFACTS_DIR/plan.md`
- [ ] Plan covers all requirements from interview
- [ ] Task ordering is correct
