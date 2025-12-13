#!/bin/bash
set -e

echo "=========================================="
echo "FlowPilot Global Tool Integration Test"
echo "=========================================="
echo ""

# Test 1: Verify flowpilot command is available
echo "[TEST 1] Verifying flowpilot command is available..."
if ! command -v flowpilot &> /dev/null; then
    echo "❌ FAILED: flowpilot command not found"
    exit 1
fi
echo "✅ PASSED: flowpilot command is available"
echo ""

# Test 2: Run flowpilot help and verify output
echo "[TEST 2] Running 'flowpilot' (should show help)..."
output=$(flowpilot 2>&1)
if [[ ! $output == *"FlowPilot CLI"* ]]; then
    echo "❌ FAILED: Help message not displayed"
    echo "Output: $output"
    exit 1
fi
echo "✅ PASSED: Help message displayed correctly"
echo ""

# Test 3: Run flowpilot help explicitly
echo "[TEST 3] Running 'flowpilot help'..."
output=$(flowpilot help 2>&1)
if [[ ! $output == *"FlowPilot CLI"* ]]; then
    echo "❌ FAILED: Help command did not display message"
    echo "Output: $output"
    exit 1
fi
echo "✅ PASSED: Help command works correctly"
echo ""

# Test 4: Initialize a git repository and run flowpilot init
echo "[TEST 4] Initializing git repository and running 'flowpilot init'..."
git init
git config user.email "test@flowpilot.test"
git config user.name "FlowPilot Test"

output=$(flowpilot init 2>&1)
if [[ ! $output == *"FlowPilot installed successfully"* ]]; then
    echo "❌ FAILED: Init command did not complete successfully"
    echo "Output: $output"
    exit 1
fi

# Verify installation files were created
if [ ! -d ".github/agents" ] || [ ! -f ".github/agents/flowpilot.agent.md" ]; then
    echo "❌ FAILED: .github/agents/flowpilot.agent.md not created"
    exit 1
fi

if [ ! -d ".github/instructions" ] || [ ! -f ".github/instructions/flowpilot-phase-details.instructions.md" ]; then
    echo "❌ FAILED: .github/instructions/flowpilot-phase-details.instructions.md not created"
    exit 1
fi

if [ ! -d ".flowpilot/template" ]; then
    echo "❌ FAILED: .flowpilot/template directory not created"
    exit 1
fi

echo "✅ PASSED: FlowPilot init completed and files installed"
echo ""

# Test 5: Create a new plan
echo "[TEST 5] Creating a new plan with 'flowpilot new test-plan'..."
output=$(flowpilot new test-plan 2>&1)
if [[ ! $output == *"Plan 'test-plan' created successfully"* ]]; then
    echo "❌ FAILED: New command did not create plan successfully"
    echo "Output: $output"
    exit 1
fi

# Verify plan structure was created
if [ ! -d ".flowpilot/plans/test-plan/meta" ]; then
    echo "❌ FAILED: Plan directory structure not created"
    exit 1
fi

if [ ! -f ".flowpilot/plans/test-plan/meta/state.md" ]; then
    echo "❌ FAILED: state.md not created"
    exit 1
fi

if [ ! -f ".flowpilot/plans/test-plan/meta/goal.md" ]; then
    echo "❌ FAILED: goal.md not created"
    exit 1
fi

echo "✅ PASSED: New plan created successfully"
echo ""

# Test 6: Lint should fail if goal.md hasn't been modified
echo "[TEST 6] Testing lint fails when goal.md is unchanged..."
output=$(flowpilot lint test-plan 2>&1 || true)
if [[ ! $output == *"goal.md has not been modified"* ]]; then
    echo "❌ FAILED: Lint should fail when goal.md is unchanged"
    echo "Output: $output"
    exit 1
fi
echo "✅ PASSED: Lint correctly fails when goal.md is unchanged"
echo ""

# Test 7: Modify goal.md and commit
echo "[TEST 7] Modifying goal.md and committing..."
echo "# Test Plan Goal" > .flowpilot/plans/test-plan/meta/goal.md
echo "This is a test plan to validate FlowPilot functionality." >> .flowpilot/plans/test-plan/meta/goal.md
git add .
git commit -m "Initial commit with test plan and modified goal.md"
echo "✅ PASSED: Goal.md modified and committed"
echo ""

# Test 8: Lint should now pass
echo "[TEST 8] Testing lint passes with modified goal.md..."
output=$(flowpilot lint test-plan 2>&1)
if [[ $output == *"FAILED"* ]] || [[ $output == *"Error"* ]]; then
    echo "❌ FAILED: Lint should pass with modified goal.md"
    echo "Output: $output"
    exit 1
fi
echo "✅ PASSED: Lint passes with modified goal.md"
echo ""

# Test 9: Advance to references phase
echo "[TEST 9] Advancing to references phase with 'flowpilot next test-plan'..."
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"references"* ]]; then
    echo "❌ FAILED: Next command should advance to references phase"
    echo "Output: $output"
    exit 1
fi
if [ ! -f ".flowpilot/plans/test-plan/meta/references.md" ]; then
    echo "❌ FAILED: references.md not created"
    exit 1
fi
echo "✅ PASSED: Advanced to references phase"
echo ""

# Test 10: Modify references.md with valid URL and commit
echo "[TEST 10] Modifying references.md and committing..."
echo "# References" > .flowpilot/plans/test-plan/meta/references.md
echo "" >> .flowpilot/plans/test-plan/meta/references.md
echo "- [Microsoft Docs](https://docs.microsoft.com)" >> .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references"
echo "✅ PASSED: References.md modified and committed"
echo ""

# Test 11: Advance to system-analysis phase
echo "[TEST 11] Advancing to system-analysis phase..."
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"system-analysis"* ]]; then
    echo "❌ FAILED: Next command should advance to system-analysis phase"
    echo "Output: $output"
    exit 1
fi
if [ ! -f ".flowpilot/plans/test-plan/meta/system-analysis.md" ]; then
    echo "❌ FAILED: system-analysis.md not created"
    exit 1
fi
echo "✅ PASSED: Advanced to system-analysis phase"
echo ""

# Test 12: Modify system-analysis.md and commit
echo "[TEST 12] Modifying system-analysis.md and committing..."
echo "# System Analysis" > .flowpilot/plans/test-plan/meta/system-analysis.md
echo "The system consists of various components." >> .flowpilot/plans/test-plan/meta/system-analysis.md
git add .
git commit -m "Add system analysis"
echo "✅ PASSED: System-analysis.md modified and committed"
echo ""

# Test 13: Advance to key-decisions phase
echo "[TEST 13] Advancing to key-decisions phase..."
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"key-decisions"* ]]; then
    echo "❌ FAILED: Next command should advance to key-decisions phase"
    echo "Output: $output"
    exit 1
fi
if [ ! -f ".flowpilot/plans/test-plan/meta/key-decisions.md" ]; then
    echo "❌ FAILED: key-decisions.md not created"
    exit 1
fi
echo "✅ PASSED: Advanced to key-decisions phase"
echo ""

# Test 14: Modify key-decisions.md, commit, and verify lint passes
echo "[TEST 14] Modifying key-decisions.md, committing, and verifying lint..."
echo "# Key Decisions" > .flowpilot/plans/test-plan/meta/key-decisions.md
echo "Decision 1: Use Docker for testing" >> .flowpilot/plans/test-plan/meta/key-decisions.md
git add .
git commit -m "Add key decisions"

# Verify lint passes after committing
output=$(flowpilot lint test-plan 2>&1)
if [[ $output == *"FAILED"* ]] || [[ $output == *"Error"* ]]; then
    echo "❌ FAILED: Lint should pass after key-decisions committed"
    echo "Output: $output"
    exit 1
fi
echo "✅ PASSED: Key-decisions.md modified, committed, and lint passes"
echo ""

# Test 15: Create new branch (hard boundary workflow)
echo "[TEST 15] Creating new branch for phase-analysis (hard boundary workflow)..."
git checkout -b phase-planning
echo "✅ PASSED: New branch created for crossing hard boundary"
echo ""

# Test 16: Advance to phase-analysis on new branch
echo "[TEST 16] Advancing to phase-analysis on new branch..."
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"phase-analysis"* ]]; then
    echo "❌ FAILED: Next command should advance to phase-analysis phase"
    echo "Output: $output"
    exit 1
fi
if [ ! -f ".flowpilot/plans/test-plan/meta/phase-analysis.md" ]; then
    echo "❌ FAILED: phase-analysis.md not created"
    exit 1
fi
echo "✅ PASSED: Advanced to phase-analysis phase"
echo ""

# Test 17: Modify phase-analysis.md and commit
echo "[TEST 17] Modifying phase-analysis.md with phases..."
cat > .flowpilot/plans/test-plan/meta/phase-analysis.md <<'EOF'
## Phase Analysis

### phase_1

**Goal**: Setup infrastructure

**Key Outcomes**:
- Infrastructure configured
- Environment ready

**Working State Transition**: Start with clean state, end with configured environment

---

### phase_2

**Goal**: Core implementation

**Key Outcomes**:
- Features implemented
- Tests passing

**Working State Transition**: Start with configured environment, end with working features

---

### phase_3

**Goal**: Testing and validation

**Key Outcomes**:
- All tests passing
- System validated

**Working State Transition**: Start with working features, end with validated system
EOF
git add .
git commit -m "Add phase analysis"
echo "✅ PASSED: Phase-analysis.md modified and committed"
echo ""

# Test 17b: Merge phase-planning (with phase-analysis) to master before advancing to phase-details
echo "[TEST 17b] Merging phase-planning to master..."
git checkout master
git merge --no-ff phase-planning -m "Merge phase analysis"
echo "DEBUG: state.md on master after merging phase-planning:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
echo "✅ PASSED: Phase-planning merged to master"
echo ""

# Test 18: Create new branch and advance to phase-details phase
echo "[TEST 18] Creating phase-details branch and advancing..."
git checkout -b phase-details
set +e  # Temporarily allow errors
output=$(flowpilot next test-plan 2>&1)
exit_code=$?
set -e  # Re-enable exit on error
echo "DEBUG: flowpilot next exit code: $exit_code"
echo "DEBUG: flowpilot next output:"
echo "$output"
echo ""
echo "DEBUG: state.md after flowpilot next:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
if [[ ! $output == *"phase-n-details"* ]]; then
    echo "❌ FAILED: Should advance to phase-details"
    exit 1
fi
# Should create phase-1-details.md, phase-2-details.md, phase-3-details.md in plan/ directory
if [ ! -f ".flowpilot/plans/test-plan/plan/phase-1-details.md" ]; then
    echo "❌ FAILED: phase-1-details.md not created"
    exit 1
fi
if [ ! -f ".flowpilot/plans/test-plan/plan/phase-2-details.md" ]; then
    echo "❌ FAILED: phase-2-details.md not created"
    exit 1
fi
if [ ! -f ".flowpilot/plans/test-plan/plan/phase-3-details.md" ]; then
    echo "❌ FAILED: phase-3-details.md not created"
    exit 1
fi
# Commit the state transition
git add .
git commit -m "Advance to phase-details phase"
echo "DEBUG: state.md after committing:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
echo "✅ PASSED: Phase details files created"
echo ""

# Test 19: Modify phase details files and commit
echo "[TEST 19] Modifying phase details files..."
echo "# Phase 1: Setup Details" > .flowpilot/plans/test-plan/plan/phase-1-details.md
echo "Detailed setup steps" >> .flowpilot/plans/test-plan/plan/phase-1-details.md
echo "# Phase 2: Implementation Details" > .flowpilot/plans/test-plan/plan/phase-2-details.md
echo "Detailed implementation steps" >> .flowpilot/plans/test-plan/plan/phase-2-details.md
echo "# Phase 3: Testing Details" > .flowpilot/plans/test-plan/plan/phase-3-details.md
echo "Detailed testing steps" >> .flowpilot/plans/test-plan/plan/phase-3-details.md
git add .
git commit -m "Add phase details content"
echo "✅ PASSED: Phase details modified and committed"
echo ""

# Simulate PR merge: merge phase-details back to master to reset merge-base
echo "[TEST 19b] Simulating PR merge: merging phase-details to master..."
git checkout master
git merge --no-ff phase-details -m "Merge phase details"
echo "DEBUG: state.md on master after merging phase-details:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
echo "✅ PASSED: Phase details merged to master"
echo ""

# Test 20: Advance to phase 1 implementation
echo "[TEST 20] Advancing to phase 1 implementation..."
git checkout -b phase-1-implementation
echo "DEBUG: state.md before flowpilot next:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
output=$(flowpilot next test-plan 2>&1)
echo "DEBUG: flowpilot next output:"
echo "$output"
echo "DEBUG: state.md after flowpilot next:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
if [[ ! $output == *"phase_1"* ]] && [[ ! $output == *"Phase 1"* ]]; then
    echo "❌ FAILED: Should advance to phase 1 implementation"
    echo "Output: $output"
    exit 1
fi
git add .
git commit -m "Advance to phase 1"
# Simulate doing the implementation work
echo "Phase 1 implementation work done" >> .flowpilot/plans/test-plan/plan/phase-1-details.md
git add .
git commit -m "Complete phase 1 implementation"
echo "✅ PASSED: Advanced to phase 1 implementation"
echo ""

# Test 21: Merge phase 1 to master
echo "[TEST 21] Merging phase 1 to master..."
git checkout master
git merge --no-ff phase-1-implementation -m "Merge phase 1"
echo "✅ PASSED: Phase 1 merged to master"
echo ""
echo "DEBUG: state.md after merging phase 1:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""

# Test 22: Advance to phase 2 implementation
echo "[TEST 22] Advancing to phase 2 implementation..."
git checkout -b phase-2-implementation
echo "DEBUG: state.md on phase-2-implementation branch:"
cat .flowpilot/plans/test-plan/meta/state.md
echo ""
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"phase_2"* ]] && [[ ! $output == *"Phase 2"* ]]; then
    echo "❌ FAILED: Should advance to phase 2 implementation"
    echo "Output: $output"
    exit 1
fi
git add .
git commit -m "Advance to phase 2"
# Simulate doing the implementation work
echo "Phase 2 implementation work done" >> .flowpilot/plans/test-plan/plan/phase-2-details.md
git add .
git commit -m "Complete phase 2 implementation"
echo "✅ PASSED: Advanced to phase 2 implementation"
echo ""

# Test 23: Merge phase 2 to master
echo "[TEST 23] Merging phase 2 to master..."
git checkout master
git merge --no-ff phase-2-implementation -m "Merge phase 2"
echo "✅ PASSED: Phase 2 merged to master"
echo ""

# Test 24: Advance to phase 3 implementation
echo "[TEST 24] Advancing to phase 3 implementation..."
git checkout -b phase-3-implementation
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"phase_3"* ]] && [[ ! $output == *"Phase 3"* ]]; then
    echo "❌ FAILED: Should advance to phase 3 implementation"
    echo "Output: $output"
    exit 1
fi
git add .
git commit -m "Advance to phase 3"
# Simulate doing the implementation work
echo "Phase 3 implementation work done" >> .flowpilot/plans/test-plan/plan/phase-3-details.md
git add .
git commit -m "Complete phase 3 implementation"
echo "✅ PASSED: Advanced to phase 3 and completed"
echo ""

# Test 25: Verify plan is complete
echo "[TEST 25] Verifying plan is complete..."
output=$(flowpilot next test-plan 2>&1)
if [[ ! $output == *"complete"* ]] && [[ ! $output == *"finished"* ]]; then
    echo "❌ FAILED: Should indicate plan is complete"
    echo "Output: $output"
    exit 1
fi
echo "✅ PASSED: Plan marked as complete"
echo ""


echo "=========================================="
echo "✅ ALL TESTS PASSED (25/25)"
echo "=========================================="
echo ""
echo "Summary:"
echo "  - Basic commands: init, new, help"
echo "  - Linting validation"
echo "  - State transitions through all phases"
echo "  - Hard boundary enforcement with PR merges"
echo "  - Template file creation"
echo "  - Complete plan workflow"
echo ""
exit 0
