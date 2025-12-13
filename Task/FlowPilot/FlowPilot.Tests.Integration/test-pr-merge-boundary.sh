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

# Set initial state (all unchecked for planning tests)
cat > .flowpilot/plans/test-plan/meta/state.md <<'EOF'
- [ ] [state] FlowPilot plan initialized
- [ ] [references] meta/references.md drafted with initial sources
- [ ] [system-analysis] meta/system-analysis.md describes relevant system parts
- [ ] [key-decisions] meta/key-decisions.md lists decision points and options
- [ ] [phase-analysis] meta/phase-analysis.md defines high-level phases
- [ ] [phase-n-details] plan/phase-n-details.md files created for each defined phase
- [ ] [phase_1] phase_1
- [ ] [phase_2] phase_2
- [ ] [phase_3] phase_3
EOF

git add .
git commit -m "Initial test plan with planning state"

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
sed -i 's/- \[ \] \[state\]/- [x] [state]/' .flowpilot/plans/test-plan/meta/state.md
git add .
git commit -m "Advance state checkbox with implementation changes"
if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "❌ FAILED: Should pass with single committed change"
  fail_count=$((fail_count + 1))
else
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
fi
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
sed -i 's/- \[ \] \[state\]/- [x] [state]/' .flowpilot/plans/test-plan/meta/state.md
git add .flowpilot/plans/test-plan/meta/state.md
if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
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
sed -i 's/- \[ \] \[state\]/- [x] [state]/' .flowpilot/plans/test-plan/meta/state.md
git add .flowpilot/plans/test-plan/meta/state.md
if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
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
sed -i 's/- \[ \] \[state\]/- [x] [state]/' .flowpilot/plans/test-plan/meta/state.md
sed -i 's/- \[ \] \[references\]/- [x] [references]/' .flowpilot/plans/test-plan/meta/state.md
git add .
git commit -m "Advance two checkboxes"
if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
  echo "✅ PASSED"
  pass_count=$((pass_count + 1))
else
  echo "❌ FAILED: Should fail with two committed changes"
  fail_count=$((fail_count + 1))
fi
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
sed -i 's/- \[ \] \[state\]/- [x] [state]/' .flowpilot/plans/test-plan/meta/state.md
sed -i 's/- \[ \] \[references\]/- [x] [references]/' .flowpilot/plans/test-plan/meta/state.md
git add .flowpilot/plans/test-plan/meta/state.md
if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
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
sed -i 's/- \[ \] \[state\]/- [x] [state]/' .flowpilot/plans/test-plan/meta/state.md
git add .flowpilot/plans/test-plan/meta/state.md
git commit -m "Advance state"
# Add more file changes (staged)
echo "Additional docs" > docs/CONTRIBUTING.md
git add docs/CONTRIBUTING.md
sed -i 's/- \[ \] \[references\]/- [x] [references]/' .flowpilot/plans/test-plan/meta/state.md
git add .flowpilot/plans/test-plan/meta/state.md
if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
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
cat > .flowpilot/plans/test-plan/meta/state.md <<'EOF'
- [x] [state] FlowPilot plan initialized
- [x] [references] meta/references.md drafted with initial sources
- [x] [system-analysis] meta/system-analysis.md describes relevant system parts
- [x] [key-decisions] meta/key-decisions.md lists decision points and options
- [x] [phase-analysis] meta/phase-analysis.md defines high-level phases
- [x] [phase-n-details] plan/phase-n-details.md files created for each defined phase
- [ ] [phase_1] phase_1
- [ ] [phase_2] phase_2
- [ ] [phase_3] phase_3
EOF
git add .flowpilot/plans/test-plan/meta/state.md
git commit -m "Planning complete - ready for implementation"

# Test 7-12: Boundary tests
for i in 7 8 9 10 11 12; do
  test_count=$((test_count + 1))
  case $i in
    7) name="Boundary_Pass_CommittedOnly"; should_pass=true; change_type="committed" ;;
    8) name="Boundary_Pass_StagedOnly"; should_pass=true; change_type="staged" ;;
    9) name="Boundary_Pass_CommittedAndStaged"; should_pass=true; change_type="both" ;;
    10) name="Boundary_Fail_CommittedOnly"; should_pass=false; change_type="committed" ;;
    11) name="Boundary_Fail_StagedOnly"; should_pass=false; change_type="staged" ;;
    12) name="Boundary_Fail_CommittedAndStaged"; should_pass=false; change_type="both" ;;
  esac
  
  echo "[TEST $test_count] $name..."
  git checkout -b test-$i
  
  # Add other file changes to simulate real PR
  mkdir -p src test
  echo "Phase implementation code $i" > "src/phase1-feature-$i.js"
  echo "Test file $i" > "test/test-$i.spec.js"
  
  if [ "$should_pass" = "true" ]; then
    sed -i 's/- \[ \] \[phase_1\]/- [x] [phase_1]/' .flowpilot/plans/test-plan/meta/state.md
  else
    sed -i 's/- \[ \] \[phase_1\]/- [x] [phase_1]/' .flowpilot/plans/test-plan/meta/state.md
    sed -i 's/- \[ \] \[phase_2\]/- [x] [phase_2]/' .flowpilot/plans/test-plan/meta/state.md
  fi
  
  if [ "$change_type" = "committed" ]; then
    git add .
    git commit -m "Changes"
  elif [ "$change_type" = "staged" ]; then
    git add "src/phase1-feature-$i.js" "test/test-$i.spec.js"
    git commit -m "Add implementation files"
    git add .flowpilot/plans/test-plan/meta/state.md
  else
    sed -i 's/- \[ \] \[phase_1\]/- [x] [phase_1]/' .flowpilot/plans/test-plan/meta/state.md
    git add .
    git commit -m "First change"
    if [ "$should_pass" = "false" ]; then
      echo "More changes $i" >> "src/phase1-feature-$i.js"
      sed -i 's/- \[ \] \[phase_2\]/- [x] [phase_2]/' .flowpilot/plans/test-plan/meta/state.md
      git add .
    fi
  fi
  
  if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
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
  
  git reset --hard HEAD 2>/dev/null || true
  git checkout master
  git branch -D test-$i
  echo ""
done

# Setup for implementation tests
echo "Setting up implementation state..."
cat > .flowpilot/plans/test-plan/meta/state.md <<'EOF'
- [x] [state] FlowPilot plan initialized
- [x] [references] meta/references.md drafted with initial sources
- [x] [system-analysis] meta/system-analysis.md describes relevant system parts
- [x] [key-decisions] meta/key-decisions.md lists decision points and options
- [x] [phase-analysis] meta/phase-analysis.md defines high-level phases
- [x] [phase-n-details] plan/phase-n-details.md files created for each defined phase
- [x] [phase_1] phase_1
- [ ] [phase_2] phase_2
- [ ] [phase_3] phase_3
EOF
git add .flowpilot/plans/test-plan/meta/state.md
git commit -m "Phase 1 complete - in implementation"

# Test 13-18: Implementation tests
for i in 13 14 15 16 17 18; do
  test_count=$((test_count + 1))
  case $i in
    13) name="Implementation_Pass_CommittedOnly"; should_pass=true; change_type="committed" ;;
    14) name="Implementation_Pass_StagedOnly"; should_pass=true; change_type="staged" ;;
    15) name="Implementation_Pass_CommittedAndStaged"; should_pass=true; change_type="both" ;;
    16) name="Implementation_Fail_CommittedOnly"; should_pass=false; change_type="committed" ;;
    17) name="Implementation_Fail_StagedOnly"; should_pass=false; change_type="staged" ;;
    18) name="Implementation_Fail_CommittedAndStaged"; should_pass=false; change_type="both" ;;
  esac
  
  echo "[TEST $test_count] $name..."
  git checkout -b test-$i
  
  # Add other file changes to simulate real PR
  mkdir -p src test docs
  echo "Phase 2 implementation code $i" > "src/phase2-feature-$i.js"
  echo "Integration test $i" > "test/integration-$i.spec.js"
  echo "Documentation for feature $i" > "docs/feature-$i.md"
  
  if [ "$should_pass" = "true" ]; then
    sed -i 's/- \[ \] \[phase_2\]/- [x] [phase_2]/' .flowpilot/plans/test-plan/meta/state.md
  else
    sed -i 's/- \[ \] \[phase_2\]/- [x] [phase_2]/' .flowpilot/plans/test-plan/meta/state.md
    sed -i 's/- \[ \] \[phase_3\]/- [x] [phase_3]/' .flowpilot/plans/test-plan/meta/state.md
  fi
  
  if [ "$change_type" = "committed" ]; then
    git add .
    git commit -m "Changes"
  elif [ "$change_type" = "staged" ]; then
    git add "src/phase2-feature-$i.js" "test/integration-$i.spec.js" "docs/feature-$i.md"
    git commit -m "Add implementation files"
    git add .flowpilot/plans/test-plan/meta/state.md
  else
    sed -i 's/- \[ \] \[phase_2\]/- [x] [phase_2]/' .flowpilot/plans/test-plan/meta/state.md
    git add .
    git commit -m "First change"
    if [ "$should_pass" = "false" ]; then
      echo "Additional changes $i" >> "src/phase2-feature-$i.js"
      sed -i 's/- \[ \] \[phase_3\]/- [x] [phase_3]/' .flowpilot/plans/test-plan/meta/state.md
      git add .
    fi
  fi
  
  if flowpilot lint test-plan 2>&1 | grep -q "pull request merge boundary"; then
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
  
  git reset --hard HEAD 2>/dev/null || true
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
