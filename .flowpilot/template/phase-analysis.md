## Phase Analysis

<!--
FlowPilot parsing rules (v1):

- A phase starts with "### phase_<number>"
- The number defines ordering
- Sections under each phase:
    - **Goal**: <text>
    - **Key Outcomes**: list of "* outcome"
    - **Working State Transition**: <text>

The linter and next-command can rely on these headings exactly.
-->

## Phase Planning Principles

Before defining phases, ensure:

1. **Each phase achieves a complete, working state** - No phase should leave the system in a broken state
2. **Phases are independently testable** - Each phase has clear pass/fail criteria
3. **Phase size is manageable** - Target 5-15 implementation steps per phase
4. **Dependencies are explicit** - Each phase clearly states what must be done first
5. **Scope is minimal** - Each phase does the minimum needed to reach next working state
6. **Risks are identified early** - High-risk phases are broken into smaller phases
7. **Test maintenance is considered** - Account for test updates in phase scope

## Phase Scope Guidelines

**Small Phase (Recommended):**
- 5-10 implementation steps
- 1-2 hours of work
- Affects 5-15 files
- Low-medium risk
- Easy to rollback

**Medium Phase (Use with caution):**
- 10-20 implementation steps
- 2-4 hours of work
- Affects 15-30 files
- Medium risk
- Requires careful planning

**Large Phase (Avoid if possible):**
- 20+ implementation steps
- 4+ hours of work
- Affects 30+ files
- High risk
- Should be split into smaller phases

## Ripple Effect Analysis

Before finalizing phases, analyze ripple effects:

**If changing database schema:**
- [ ] Entity classes affected
- [ ] Repository methods affected
- [ ] Handlers affected
- [ ] API contracts affected
- [ ] Tests affected
- [ ] Migrations needed

**If changing authentication:**
- [ ] Middleware affected
- [ ] Endpoints affected
- [ ] Test fixtures affected
- [ ] Client code affected
- [ ] Cookie/token handling affected

**If changing domain entities:**
- [ ] Handlers querying the entity
- [ ] Mappers using the entity
- [ ] Specifications using the entity
- [ ] Tests using the entity
- [ ] Related entities affected

## Phase Definitions

### phase_1

**Goal**: [One sentence describing the phase goal]

**Key Outcomes**:
* Outcome 1 - specific, measurable
* Outcome 2 - specific, measurable
* Outcome 3 - specific, measurable

**Working State Transition**: [Describe what changes from working state A to working state B. Both states must be working states with all tests passing]

**Scope Size:** Small/Medium/Large
**Risk Level:** Low/Medium/High
**Dependencies:** [List phases or external work that must complete first]
**Ripple Effects:** [List areas of code that will be affected]

---

### phase_2

**Goal**: [One sentence describing the phase goal]

**Key Outcomes**:
* Outcome 1 - specific, measurable
* Outcome 2 - specific, measurable

**Working State Transition**: [Describe the working state transition]

**Scope Size:** Small/Medium/Large
**Risk Level:** Low/Medium/High
**Dependencies:** [Typically "phase_1 completed"]
**Ripple Effects:** [List areas of code that will be affected]

---

[Continue with additional phases...]

## Phase Validation Checklist

Before finalizing the phase analysis, verify:

- [ ] Each phase has 3-5 key outcomes (not too many, not too few)
- [ ] Each phase has a clear working state transition
- [ ] No phase is too large (>20 steps indicates need to split)
- [ ] High-risk phases are broken into smaller increments
- [ ] Test maintenance is accounted for in phases
- [ ] Rollback is possible at the end of any phase
- [ ] Dependencies between phases are clear and minimal
- [ ] The sequence progresses from low-risk to high-risk where possible

### phase_1

**Goal**: update me

**Key Outcomes**:
- outcome 1
- outcome 2

**Working State Transition**: update me
<!-- ("How does splitting here help go from working state, to working state?") -->

---

### phase_2

**Goal**: update me

**Key Outcomes**:
- outcome 1
- outcome 2

**Working State Transition**: update me
<!-- ("How does splitting here help go from working state, to working state?") -->

---