#!/bin/bash
set -e

echo "=========================================="
echo "PullRequestMergeBoundary Linting Tests"
echo "=========================================="
echo ""

# Initialize git repository
git init
git config user.email "test@flowpilot.test"
git config user.name "FlowPilot Test"

# Install FlowPilot
flowpilot init
git add .
git commit -m "Initial FlowPilot installation"

# Create a test plan
flowpilot new test-plan
echo "# Test Goal" > .flowpilot/plans/test-plan/meta/goal.md
git add .
git commit -m "Initial test plan with goal"

test_count=0
pass_count=0
fail_count=0

# Test 1: Planning_Fail_TwoTransitions
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Fail_TwoTransitions..."
git checkout -b test-1
# Add other file changes to simulate real PR
echo "Additional implementation notes" >> .flowpilot/plans/test-plan/meta/goal.md
mkdir -p src
echo "// Implementation code" > src/implementation.js
git add .
git commit -m "Add implementation changes"
# Make first transition
flowpilot next test-plan
git add .
git commit -m "Advance to references"
# Try second transition (should fail due to boundary)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "✅ PASSED: Correctly blocked second transition"
  pass_count=$((pass_count + 1))
  
  # Verify that git working tree and index are clean after failed transition
  # We check git status for "nothing to commit, working tree clean" message
  test_count=$((test_count + 1))
  if git status | grep -q "nothing to commit, working tree clean"; then
    echo "✅ PASSED: git working tree and index are clean after failed transition"
    pass_count=$((pass_count + 1))
  else
    echo "❌ FAILED: git working tree and index should be clean after failed transition"
    echo "Git status after failed transition:"
    git status
    fail_count=$((fail_count + 1))
  fi
else
  echo "❌ FAILED: Should block second transition"
  fail_count=$((fail_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-1
echo ""

# Test 2: Planning_Pass_StagedOnly
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Pass_StagedOnly..."
git checkout -b test-2
# Add other file changes to simulate real PR
echo "More implementation notes" >> .flowpilot/plans/test-plan/meta/goal.md
mkdir -p src
echo "// More code" > src/utils.js
git add .flowpilot/plans/test-plan/meta/goal.md src/utils.js
git commit -m "Add implementation files"
# Use flowpilot next to advance state (leaves state.md staged)
# Test flowpilot next (should succeed without boundary error)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "❌ FAILED: Should pass with single staged change"
  fail_count=$((fail_count + 1))
else
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-2
echo ""

# Test 3: Planning_Pass_CommittedAndStaged
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Pass_CommittedAndStaged..."
git checkout -b test-3
# Add other file changes to simulate real PR
echo "Test documentation" > .flowpilot/plans/test-plan/meta/notes.md
git add .flowpilot/plans/test-plan/meta/notes.md
git commit -m "Add notes"
# Use flowpilot next to advance state (leaves state.md staged)
# Test flowpilot next (should succeed without boundary error)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "❌ FAILED: Should pass with single change (staged)"
  fail_count=$((fail_count + 1))
else
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-3
echo ""

# Test 4: Planning_Fail_CommittedOnly
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Fail_CommittedOnly..."
git checkout -b test-4
# Add other file changes to simulate real PR
echo "Additional context" >> .flowpilot/plans/test-plan/meta/goal.md
git add .
git commit -m "Add context"
# Use flowpilot next twice to advance two phases
flowpilot next test-plan
git add .
git commit -m "Advance to references"
printf "# References\n- [Test](https://example.com)\n" > .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references content"
# Test flowpilot next (should fail with boundary error)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
else
  echo "❌ FAILED: Should fail with two committed changes"
  fail_count=$((fail_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-4
echo ""

# Test 5: Planning_Fail_StagedOnly
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Fail_StagedOnly..."
git checkout -b test-5
# Add other file changes to simulate real PR
mkdir -p config
echo "Configuration changes" > config/config.json
git add config/config.json
git commit -m "Add config"
# Use flowpilot next twice to advance two phases (second one stays staged)
flowpilot next test-plan
git add .
git commit -m "Advance to references"
printf "# References\n- [Test](https://example.com)\n" > .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references content"
# Test flowpilot next (should fail with boundary error)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
else
  echo "❌ FAILED: Should fail with two staged changes"
  fail_count=$((fail_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-5
echo ""

# Test 6: Planning_Fail_CommittedAndStaged
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Fail_CommittedAndStaged..."
git checkout -b test-6
# Add other file changes to simulate real PR (committed)
mkdir -p docs
echo "README update" > docs/README.md
git add docs/README.md
git commit -m "Add README"
# Use flowpilot next to advance first phase
flowpilot next test-plan
git add .
git commit -m "Advance to references"
printf "# References\n- [Test](https://example.com)\n" > .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references content"
# Add more file changes (staged)
echo "Additional docs" > docs/CONTRIBUTING.md
git add docs/CONTRIBUTING.md
# Test flowpilot next (should fail with boundary error)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
else
  echo "❌ FAILED: Should fail with one committed + one staged change"
  fail_count=$((fail_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-6
echo ""

# Setup for boundary tests - advance planning to completion
echo "Setting up boundary state..."
# Branch 1: references
git checkout -b branch-references
flowpilot next test-plan
git add .
git commit -m "Advance to references"
printf "# References\n- [Test](https://example.com)\n" > .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references content"
git checkout master
git merge --no-ff branch-references -m "Merge references"

# Branch 2: system-analysis
git checkout -b branch-system-analysis
flowpilot next test-plan
git add .
git commit -m "Advance to system-analysis"
printf "# System Analysis\nSystem details here\n" > .flowpilot/plans/test-plan/meta/system-analysis.md
git add .
git commit -m "Add system analysis content"
git checkout master
git merge --no-ff branch-system-analysis -m "Merge system-analysis"

# Branch 3: key-decisions
git checkout -b branch-key-decisions
flowpilot next test-plan
git add .
git commit -m "Advance to key-decisions"
printf "# Key Decisions\nDecision details here\n" > .flowpilot/plans/test-plan/meta/key-decisions.md
git add .
git commit -m "Add key decisions content"
git checkout master
git merge --no-ff branch-key-decisions -m "Merge key-decisions"

# Branch 4: phase-analysis
git checkout -b branch-phase-analysis
flowpilot next test-plan
git add .
git commit -m "Advance to phase-analysis"
cat > .flowpilot/plans/test-plan/meta/phase-analysis.md <<'EOF'
## Phase Analysis

### phase_1
**Goal**: Implement feature 1
**Key Outcomes**: Feature 1 complete

### phase_2
**Goal**: Implement feature 2
**Key Outcomes**: Feature 2 complete

### phase_3
**Goal**: Implement feature 3
**Key Outcomes**: Feature 3 complete
EOF
git add .
git commit -m "Add phase analysis content"
git checkout master
git merge --no-ff branch-phase-analysis -m "Merge phase-analysis"

# Branch 5: phase-details
git checkout -b branch-phase-details
# PhaseDetailsTransition adds multiple NEW checkboxes (phase_1, phase_2, phase_3)
# These are pure additions and should be allowed (only modifications count toward limit)
flowpilot next test-plan
git add .
git commit -m "Advance to phase-details"
printf "# Phase 1 Details\nDetails here\n" > .flowpilot/plans/test-plan/plan/phase-1-details.md
printf "# Phase 2 Details\nDetails here\n" > .flowpilot/plans/test-plan/plan/phase-2-details.md
printf "# Phase 3 Details\nDetails here\n" > .flowpilot/plans/test-plan/plan/phase-3-details.md
git add .
git commit -m "Add phase details content"
git checkout master
git merge --no-ff branch-phase-details -m "Merge phase-details"

# Summary
echo ""
echo "=========================================="
echo "Test Summary"
echo "=========================================="
echo "Total tests: $test_count"
echo "Passed: $pass_count"
echo "Failed: $fail_count"
echo ""

if [ $fail_count -eq 0 ]; then
  echo "✅ ALL TESTS PASSED ($pass_count/$test_count)"
  echo "=========================================="
  exit 0
else
  echo "❌ SOME TESTS FAILED ($pass_count passed, $fail_count failed out of $test_count)"
  echo "=========================================="
  exit 1
fi
