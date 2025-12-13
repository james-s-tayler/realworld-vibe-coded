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
git commit -m "Initial test plan"

# Helper function to reset state.md to a specific configuration
reset_state() {
  local state_config=$1
  cat > .flowpilot/plans/test-plan/meta/state.md <<EOF
$state_config
EOF
}

# Helper function to advance one checkbox (from [ ] to [x])
advance_one_checkbox() {
  local from=$1
  sed -i "s/- \[ \] \[$from\]/- [x] [$from]/" .flowpilot/plans/test-plan/meta/state.md
}

# Helper function to advance two checkboxes
advance_two_checkboxes() {
  local from1=$1
  local from2=$2
  sed -i "s/- \[ \] \[$from1\]/- [x] [$from1]/" .flowpilot/plans/test-plan/meta/state.md
  sed -i "s/- \[ \] \[$from2\]/- [x] [$from2]/" .flowpilot/plans/test-plan/meta/state.md
}

# Planning phase initial state
PLANNING_INITIAL="- [ ] [state] FlowPilot plan initialized
- [ ] [references] meta/references.md drafted with initial sources
- [ ] [system-analysis] meta/system-analysis.md describes relevant system parts
- [ ] [key-decisions] meta/key-decisions.md lists decision points and options
- [ ] [phase-analysis] meta/phase-analysis.md defines high-level phases
- [ ] [phase-n-details] plan/phase-n-details.md files created for each defined phase
- [ ] [phase_1] phase_1
- [ ] [phase_2] phase_2
- [ ] [phase_3] phase_3"

# Boundary initial state (planning complete, ready to start implementation)
BOUNDARY_INITIAL="- [x] [state] FlowPilot plan initialized
- [x] [references] meta/references.md drafted with initial sources
- [x] [system-analysis] meta/system-analysis.md describes relevant system parts
- [x] [key-decisions] meta/key-decisions.md lists decision points and options
- [x] [phase-analysis] meta/phase-analysis.md defines high-level phases
- [x] [phase-n-details] plan/phase-n-details.md files created for each defined phase
- [ ] [phase_1] phase_1
- [ ] [phase_2] phase_2
- [ ] [phase_3] phase_3"

# Implementation initial state (phase 1 complete)
IMPLEMENTATION_INITIAL="- [x] [state] FlowPilot plan initialized
- [x] [references] meta/references.md drafted with initial sources
- [x] [system-analysis] meta/system-analysis.md describes relevant system parts
- [x] [key-decisions] meta/key-decisions.md lists decision points and options
- [x] [phase-analysis] meta/phase-analysis.md defines high-level phases
- [x] [phase-n-details] plan/phase-n-details.md files created for each defined phase
- [x] [phase_1] phase_1
- [ ] [phase_2] phase_2
- [ ] [phase_3] phase_3"

test_count=0
pass_count=0
fail_count=0

run_test() {
  local test_name=$1
  local should_pass=$2
  local phase=$3
  local change_type=$4

  test_count=$((test_count + 1))
  echo "[TEST $test_count] $test_name..."

  # Create a fresh branch for this test
  git checkout master 2>/dev/null || git checkout -b master

  # Set initial state based on phase and commit to master first
  case $phase in
    "planning")
      reset_state "$PLANNING_INITIAL"
      ;;
    "boundary")
      reset_state "$BOUNDARY_INITIAL"
      ;;
    "implementation")
      reset_state "$IMPLEMENTATION_INITIAL"
      ;;
  esac

  git add .flowpilot/plans/test-plan/meta/state.md
  git commit -m "Reset to $phase initial state for test $test_count"

  # Now create branch for the test - this establishes the merge-base
  git checkout -b "test-$test_count"

  # Make changes based on should_pass flag and change_type
  if [ "$should_pass" = "true" ]; then
    # Pass: only one checkbox change
    case $phase in
      "planning")
        advance_one_checkbox "state"
        ;;
      "boundary")
        advance_one_checkbox "phase_1"
        ;;
      "implementation")
        advance_one_checkbox "phase_2"
        ;;
    esac
  else
    # Fail: two checkbox changes
    case $phase in
      "planning")
        advance_two_checkboxes "state" "references"
        ;;
      "boundary")
        advance_two_checkboxes "phase_1" "phase_2"
        ;;
      "implementation")
        advance_two_checkboxes "phase_2" "phase_3"
        ;;
    esac
  fi

  # Apply changes based on change_type
  case $change_type in
    "committed")
      git add .flowpilot/plans/test-plan/meta/state.md
      git commit -m "Change for test $test_count"
      ;;
    "staged")
      git add .flowpilot/plans/test-plan/meta/state.md
      ;;
    "both")
      # First make and commit one change
      if [ "$should_pass" = "true" ]; then
        # For pass case with 'both', we still only want one total change
        # So just stage the one change
        git add .flowpilot/plans/test-plan/meta/state.md
      else
        # For fail case with 'both', commit one change then stage another
        # First undo our two changes
        git checkout .flowpilot/plans/test-plan/meta/state.md
        # Make first change and commit
        case $phase in
          "planning")
            advance_one_checkbox "state"
            ;;
          "boundary")
            advance_one_checkbox "phase_1"
            ;;
          "implementation")
            advance_one_checkbox "phase_2"
            ;;
        esac
        git add .flowpilot/plans/test-plan/meta/state.md
        git commit -m "First change for test $test_count"
        # Make second change and stage
        case $phase in
          "planning")
            advance_one_checkbox "references"
            ;;
          "boundary")
            advance_one_checkbox "phase_2"
            ;;
          "implementation")
            advance_one_checkbox "phase_3"
            ;;
        esac
        git add .flowpilot/plans/test-plan/meta/state.md
      fi
      ;;
  esac

  # Run lint and check result
  set +e
  output=$(flowpilot lint test-plan 2>&1)
  lint_exit_code=$?
  set -e

  # Check if the result matches expectation
  if [ "$should_pass" = "true" ]; then
    # Should pass - lint should succeed (exit code 0) and not contain error message
    if [ $lint_exit_code -eq 0 ] && [[ ! $output == *"pull request merge boundary"* ]]; then
      echo "✅ PASSED: $test_name"
      pass_count=$((pass_count + 1))
    else
      echo "❌ FAILED: $test_name - Expected lint to pass but it failed"
      echo "Exit code: $lint_exit_code"
      echo "Output: $output"
      fail_count=$((fail_count + 1))
    fi
  else
    # Should fail - lint should fail with the boundary message
    if [[ $output == *"pull request merge boundary"* ]]; then
      echo "✅ PASSED: $test_name"
      pass_count=$((pass_count + 1))
    else
      echo "❌ FAILED: $test_name - Expected lint to fail with boundary message but it didn't"
      echo "Exit code: $lint_exit_code"
      echo "Output: $output"
      fail_count=$((fail_count + 1))
    fi
  fi
  echo ""

  # Clean up - go back to master, discard staged changes
  git reset --hard HEAD 2>/dev/null || true
  git checkout master
  git branch -D "test-$test_count" 2>/dev/null || true
}

# Planning Phase Tests (6 tests)
echo "=== Planning Phase Tests ==="
run_test "Planning_Pass_CommittedOnly" "true" "planning" "committed"
run_test "Planning_Pass_StagedOnly" "true" "planning" "staged"
run_test "Planning_Pass_CommittedAndStaged" "true" "planning" "both"
run_test "Planning_Fail_CommittedOnly" "false" "planning" "committed"
run_test "Planning_Fail_StagedOnly" "false" "planning" "staged"
run_test "Planning_Fail_CommittedAndStaged" "false" "planning" "both"
echo ""

# Boundary Phase Tests (6 tests)
echo "=== Boundary Phase Tests ==="
run_test "Boundary_Pass_CommittedOnly" "true" "boundary" "committed"
run_test "Boundary_Pass_StagedOnly" "true" "boundary" "staged"
run_test "Boundary_Pass_CommittedAndStaged" "true" "boundary" "both"
run_test "Boundary_Fail_CommittedOnly" "false" "boundary" "committed"
run_test "Boundary_Fail_StagedOnly" "false" "boundary" "staged"
run_test "Boundary_Fail_CommittedAndStaged" "false" "boundary" "both"
echo ""

# Implementation Phase Tests (6 tests)
echo "=== Implementation Phase Tests ==="
run_test "Implementation_Pass_CommittedOnly" "true" "implementation" "committed"
run_test "Implementation_Pass_StagedOnly" "true" "implementation" "staged"
run_test "Implementation_Pass_CommittedAndStaged" "true" "implementation" "both"
run_test "Implementation_Fail_CommittedOnly" "false" "implementation" "committed"
run_test "Implementation_Fail_StagedOnly" "false" "implementation" "staged"
run_test "Implementation_Fail_CommittedAndStaged" "false" "implementation" "both"
echo ""

# Summary
echo "=========================================="
if [ $fail_count -eq 0 ]; then
  echo "✅ ALL TESTS PASSED ($pass_count/$test_count)"
else
  echo "❌ SOME TESTS FAILED ($pass_count passed, $fail_count failed out of $test_count)"
fi
echo "=========================================="
echo ""

if [ $fail_count -gt 0 ]; then
  exit 1
fi

exit 0
