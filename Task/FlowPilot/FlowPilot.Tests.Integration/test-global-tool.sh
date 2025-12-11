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

echo "=========================================="
echo "✅ ALL TESTS PASSED"
echo "=========================================="
exit 0
