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

# Test 1: Planning_Pass_CommittedOnly
test_count=$((test_count + 1))
echo "[TEST $test_count] Planning_Pass_CommittedOnly..."
git checkout -b test-1
# Add other file changes to simulate real PR
echo "Additional implementation notes" >> .flowpilot/plans/test-plan/meta/goal.md
mkdir -p src
echo "// Implementation code" > src/implementation.js
git add .
git commit -m "Add implementation changes"
# Use flowpilot next to advance state
flowpilot next test-plan
git add .
git commit -m "Advance state with flowpilot next"
# Test flowpilot next (should succeed without boundary error)
if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "❌ FAILED: Should pass with single committed change"
  fail_count=$((fail_count + 1))
else
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
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

# Test 7: Verify flowpilot lint and flowpilot next output same boundary message
test_count=$((test_count + 1))
echo "[TEST $test_count] Verify lint and next output same boundary message..."
git checkout -b test-7
echo "Additional context" >> .flowpilot/plans/test-plan/meta/goal.md
git add .
git commit -m "Add context"
# Advance twice to trigger boundary
flowpilot next test-plan
git add .
git commit -m "Advance to references"
printf "# References\n- [Test](https://example.com)\n" > .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references content"
# Capture output from both commands
next_output=$(flowpilot next test-plan 2>&1 || true)
lint_output=$(flowpilot lint test-plan 2>&1 || true)
if echo "$next_output" | grep -q "pull request merge boundary" && echo "$lint_output" | grep -q "pull request merge boundary"; then
  echo "✅ PASSED: Both commands output boundary message"
  pass_count=$((pass_count + 1))
else
  echo "❌ FAILED: Commands should output same boundary message"
  echo "flowpilot next output: $next_output"
  echo "flowpilot lint output: $lint_output"
  fail_count=$((fail_count + 1))
fi
git reset --hard HEAD
git checkout master
git branch -D test-7
echo ""

# Setup for boundary tests - advance planning to completion
echo "Setting up boundary state..."
# Advance through all planning phases
# First transition to references phase
flowpilot next test-plan
git add .
git commit -m "Advance to references"
# Fill in references content
printf "# References\n- [Test](https://example.com)\n" > .flowpilot/plans/test-plan/meta/references.md
git add .
git commit -m "Add references content"
# Transition to system-analysis
flowpilot next test-plan
git add .
git commit -m "Advance to system-analysis"
# Fill in system-analysis content
printf "# System Analysis\nSystem details here\n" > .flowpilot/plans/test-plan/meta/system-analysis.md
git add .
git commit -m "Add system analysis content"
# Transition to key-decisions
flowpilot next test-plan
git add .
git commit -m "Advance to key-decisions"
# Fill in key-decisions content
printf "# Key Decisions\nDecision details here\n" > .flowpilot/plans/test-plan/meta/key-decisions.md
git add .
git commit -m "Add key decisions content"

# Now at hard boundary - need new branch for phase-analysis
git checkout -b planning-analysis
# Transition to phase-analysis
flowpilot next test-plan
git add .
git commit -m "Advance to phase-analysis"
# Fill in phase-analysis content
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
# Merge back to master
git checkout master
git merge --no-ff planning-analysis -m "Merge phase-analysis"

# Now another boundary - need new branch for phase-details
git checkout -b planning-details
# Transition to phase-details
flowpilot next test-plan
git add .
git commit -m "Advance to phase-details"
# Fill in phase details content
echo "# Phase 1 Details" > .flowpilot/plans/test-plan/plan/phase-1-details.md
echo "# Phase 2 Details" > .flowpilot/plans/test-plan/plan/phase-2-details.md
echo "# Phase 3 Details" > .flowpilot/plans/test-plan/plan/phase-3-details.md
git add .
git commit -m "Add phase details content"
# Merge back to master to reset merge-base
git checkout master
git merge --no-ff planning-details -m "Merge phase-details"

# Test 8-13: Boundary tests (renamed to start from 8)
for i in 8 9 10 11 12 13; do
  test_count=$((test_count + 1))
  case $i in
    8) name="Boundary_Pass_CommittedOnly"; should_pass=true; change_type="committed" ;;
    9) name="Boundary_Pass_StagedOnly"; should_pass=true; change_type="staged" ;;
    10) name="Boundary_Pass_CommittedAndStaged"; should_pass=true; change_type="both" ;;
    11) name="Boundary_Fail_CommittedOnly"; should_pass=false; change_type="committed" ;;
    12) name="Boundary_Fail_StagedOnly"; should_pass=false; change_type="staged" ;;
    13) name="Boundary_Fail_CommittedAndStaged"; should_pass=false; change_type="both" ;;
  esac
  
  echo "[TEST $test_count] $name..."
  git checkout -b test-$i
  
  # Add other file changes to simulate real PR
  mkdir -p src test
  echo "Phase implementation code $i" > "src/phase1-feature-$i.js"
  echo "Test file $i" > "test/test-$i.spec.js"
  
  if [ "$should_pass" = "true" ]; then
    # Single transition
    git add src test
    git commit -m "Add implementation files"
    if [ "$change_type" = "committed" ]; then
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_1"
    elif [ "$change_type" = "staged" ]; then
      # Test flowpilot next (stays staged)
      true
    else
      # Both: commit first transition
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_1"
    fi
  else
    # Multiple transitions (should fail)
    git add src test
    git commit -m "Add implementation files"
    if [ "$change_type" = "committed" ]; then
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_1"
      echo "More code" >> "src/phase1-feature-$i.js"
      git add .
      git commit -m "More changes"
    elif [ "$change_type" = "staged" ]; then
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_1"
      echo "More code" >> "src/phase1-feature-$i.js"
      git add .
      git commit -m "More changes"
    else
      # Both: one committed, one staged
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_1"
      echo "More code" >> "src/phase1-feature-$i.js"
      git add .
      git commit -m "More changes"
    fi
  fi
  
  # Test flowpilot next
  if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
    if [ "$should_pass" = "true" ]; then
      echo "❌ FAILED: Should pass"
      fail_count=$((fail_count + 1))
    else
      echo "✅ PASSED"
      pass_count=$((pass_count + 1))
    fi
  else
    if [ "$should_pass" = "true" ]; then
      echo "✅ PASSED"
      pass_count=$((pass_count + 1))
    else
      echo "❌ FAILED: Should fail"
      fail_count=$((fail_count + 1))
    fi
  fi
  
  git reset --hard HEAD
  git checkout master
  git branch -D test-$i
  echo ""
done

# Setup for implementation tests
echo "Setting up implementation state..."
git checkout -b phase-1-impl
flowpilot next test-plan
git add .
git commit -m "Advance to phase_1"
git checkout master
git merge --no-ff phase-1-impl -m "Merge phase 1"

# Test 14-19: Implementation tests (renamed to start from 14)
for i in 14 15 16 17 18 19; do
  test_count=$((test_count + 1))
  case $i in
    14) name="Implementation_Pass_CommittedOnly"; should_pass=true; change_type="committed" ;;
    15) name="Implementation_Pass_StagedOnly"; should_pass=true; change_type="staged" ;;
    16) name="Implementation_Pass_CommittedAndStaged"; should_pass=true; change_type="both" ;;
    17) name="Implementation_Fail_CommittedOnly"; should_pass=false; change_type="committed" ;;
    18) name="Implementation_Fail_StagedOnly"; should_pass=false; change_type="staged" ;;
    19) name="Implementation_Fail_CommittedAndStaged"; should_pass=false; change_type="both" ;;
  esac
  
  echo "[TEST $test_count] $name..."
  git checkout -b test-$i
  
  # Add other file changes to simulate real PR
  mkdir -p src test docs
  echo "Phase 2 implementation code $i" > "src/phase2-feature-$i.js"
  echo "Integration test $i" > "test/integration-$i.spec.js"
  echo "Documentation for feature $i" > "docs/feature-$i.md"
  
  if [ "$should_pass" = "true" ]; then
    # Single transition
    git add src test docs
    git commit -m "Add implementation files"
    if [ "$change_type" = "committed" ]; then
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_2"
    elif [ "$change_type" = "staged" ]; then
      # Test flowpilot next (stays staged)
      true
    else
      # Both: commit first transition
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_2"
    fi
  else
    # Multiple transitions (should fail)
    git add src test docs
    git commit -m "Add implementation files"
    if [ "$change_type" = "committed" ]; then
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_2"
      echo "More code" >> "src/phase2-feature-$i.js"
      git add .
      git commit -m "More changes"
    elif [ "$change_type" = "staged" ]; then
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_2"
      echo "More code" >> "src/phase2-feature-$i.js"
      git add .
      git commit -m "More changes"
    else
      # Both: one committed, one staged
      flowpilot next test-plan
      git add .
      git commit -m "Advance to phase_2"
      echo "More code" >> "src/phase2-feature-$i.js"
      git add .
      git commit -m "More changes"
    fi
  fi
  
  # Test flowpilot next
  if flowpilot next test-plan 2>&1 | grep -q "pull request merge boundary"; then
    if [ "$should_pass" = "true" ]; then
      echo "❌ FAILED: Should pass"
      fail_count=$((fail_count + 1))
    else
      echo "✅ PASSED"
      pass_count=$((pass_count + 1))
    fi
  else
    if [ "$should_pass" = "true" ]; then
      echo "✅ PASSED"
      pass_count=$((pass_count + 1))
    else
      echo "❌ FAILED: Should fail"
      fail_count=$((fail_count + 1))
    fi
  fi
  
  git reset --hard HEAD
  git checkout master
  git branch -D test-$i
  echo ""
done

# Summary
echo "=========================================="
if [ $fail_count -eq 0 ]; then
  echo "✅ ALL TESTS PASSED ($pass_count/$test_count)"
  echo "=========================================="
  exit 0
else
  echo "❌ SOME TESTS FAILED ($pass_count passed, $fail_count failed out of $test_count)"
  echo "=========================================="
  exit 1
fi
