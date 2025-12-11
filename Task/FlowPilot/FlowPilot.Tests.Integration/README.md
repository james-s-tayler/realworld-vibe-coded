# FlowPilot Integration Tests

Docker-based integration tests for FlowPilot CLI as a global tool.

## Overview

This test harness validates that FlowPilot can be:
1. Built from source
2. Packaged as a NuGet package
3. Published to a local NuGet feed
4. Installed as a dotnet global tool
5. Executed successfully

## Running Tests

From the repository root:

```bash
cd Task/FlowPilot/FlowPilot.Tests.Integration
docker compose build
docker compose up
```

Or run directly:

```bash
docker compose up --build
```

## Test Scenarios

The integration test validates:

1. **Command Availability**: Verifies `flowpilot` command is available after installation
2. **Help Display**: Tests that running `flowpilot` without arguments shows help
3. **Help Command**: Tests explicit `flowpilot help` command
4. **Init Command**: Initializes FlowPilot in a git repository and verifies files are created
5. **New Command**: Creates a new plan and verifies plan structure

## Exit Codes

- `0`: All tests passed
- `1`: One or more tests failed

## Adding More Tests

To add additional test scenarios, edit `test-global-tool.sh` and add new test blocks following the pattern:

```bash
echo "[TEST N] Description..."
# test code here
echo "✅ PASSED: Test description"
echo ""
```
