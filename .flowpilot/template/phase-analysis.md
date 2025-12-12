## Phase Analysis

<!--
FlowPilot parsing rules (v1):

- A phase starts with "### phase_<number>"
- The number defines ordering
- Sections under each phase:
    - **Goal**: <text>
    - **Key Outcomes**: list of "* outcome"
    - **Working State Transition**: <text>
    - **PR Boundary**: <yes|no> (optional, defaults to no)

When **PR Boundary** is set to "yes", flowpilot next will not allow advancing beyond this phase 
in the same pull request. You must review, test, and merge the PR before proceeding to the next phase.

The linter and next-command can rely on these headings exactly.
-->

### phase_1

**Goal**: update me

**Key Outcomes**:
- outcome 1
- outcome 2

**Working State Transition**: update me
<!-- ("How does splitting here help go from working state, to working state?") -->

**PR Boundary**: no

---

### phase_2

**Goal**: update me

**Key Outcomes**:
- outcome 1
- outcome 2

**Working State Transition**: update me
<!-- ("How does splitting here help go from working state, to working state?") -->

**PR Boundary**: no

---