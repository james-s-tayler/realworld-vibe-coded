#!/bin/bash
# E2E Test Runner with Sharding Support
# This script runs Playwright E2E tests with optional sharding based on namespace filtering.

set -e

PROJECT_PATH="./E2eTests/E2eTests.csproj"
TESTS_DIR="./E2eTests/Tests"

# Build base command arguments as an array for proper quoting
CMD_ARGS=(
    "dotnet" "test" "$PROJECT_PATH"
    "-c" "Release"
    "--no-build"
    "--logger" "trx;LogFileName=e2e-results.trx"
    "--results-directory" "/Reports/Test/e2e/Results"
    "--verbosity" "normal"
)

# If sharding is enabled, filter tests by namespace
if [ -n "$SHARD" ] && [ -n "$SHARD_TOTAL" ]; then
    echo "Running E2E tests with sharding: Shard $SHARD of $SHARD_TOTAL"
    
    # Dynamically discover test namespaces by scanning source files
    # This finds all unique namespaces under the Tests directory
    echo "Discovering test namespaces from source files..."
    
    # Extract unique namespaces from .cs files
    # 1. Find lines containing "namespace E2eTests.Tests"
    # 2. Extract just the namespace name (remove "namespace " prefix and any trailing characters)
    # 3. Remove trailing semicolons, whitespace, and carriage returns
    # 4. Sort uniquely
    mapfile -t NAMESPACES < <(grep -rh "namespace E2eTests.Tests" "$TESTS_DIR" --include="*.cs" 2>/dev/null | \
        sed 's/.*namespace //;s/[;[:space:]]*$//' | \
        tr -d '\r' | \
        sort -u)
    
    if [ ${#NAMESPACES[@]} -eq 0 ]; then
        echo "Error: No test namespaces found in $TESTS_DIR"
        exit 1
    fi
    
    echo "Found ${#NAMESPACES[@]} test namespaces:"
    for ns in "${NAMESPACES[@]}"; do
        echo "  - '$ns'"
    done
    
    # Calculate which namespaces belong to this shard using modulo arithmetic
    # This distributes namespaces evenly across shards
    FILTER_PARTS=()
    
    for i in "${!NAMESPACES[@]}"; do
        # Assign namespace to shard: (index % total) + 1 == current shard
        ASSIGNED_SHARD=$(( (i % SHARD_TOTAL) + 1 ))
        
        if [ "$ASSIGNED_SHARD" -eq "$SHARD" ]; then
            # Use FullyQualifiedName filter to match namespace prefix
            FILTER_PARTS+=("FullyQualifiedName~${NAMESPACES[$i]}")
            echo "  Shard $SHARD: ${NAMESPACES[$i]}"
        fi
    done
    
    # Join filter parts with OR (|)
    if [ ${#FILTER_PARTS[@]} -gt 0 ]; then
        FILTER=$(IFS="|"; echo "${FILTER_PARTS[*]}")
        echo "Test filter: $FILTER"
        CMD_ARGS+=("--filter" "$FILTER")
    else
        echo "Warning: No tests assigned to shard $SHARD"
        exit 0
    fi
else
    echo "Running all E2E tests (no sharding)"
fi

# Execute the test command (array expansion preserves proper quoting)
echo "Executing: ${CMD_ARGS[*]}"
"${CMD_ARGS[@]}"
