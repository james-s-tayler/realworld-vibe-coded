#!/bin/bash
# E2E Test Runner with Sharding Support
# This script runs Playwright E2E tests with optional sharding based on namespace filtering.

set -e

# Base dotnet test command
CMD="dotnet test ./E2eTests/E2eTests.csproj -c Release --no-build --logger trx;LogFileName=e2e-results.trx --results-directory /Reports/Test/e2e/Results --verbosity normal"

# Test namespaces organized by page (alphabetically sorted for deterministic sharding)
# Each namespace corresponds to a test directory under Tests/
NAMESPACES=(
    "E2eTests.Tests.ArticlePage"
    "E2eTests.Tests.EditorPage"
    "E2eTests.Tests.HomePage"
    "E2eTests.Tests.LoginPage"
    "E2eTests.Tests.ProfilePage"
    "E2eTests.Tests.RegisterPage"
    "E2eTests.Tests.SettingsPage"
    "E2eTests.Tests.SwaggerPage"
)

# If sharding is enabled, filter tests by namespace
if [ -n "$SHARD" ] && [ -n "$SHARD_TOTAL" ]; then
    echo "Running E2E tests with sharding: Shard $SHARD of $SHARD_TOTAL"
    
    # Calculate which namespaces belong to this shard using modulo arithmetic
    # This distributes namespaces evenly across shards
    FILTER_PARTS=()
    
    for i in "${!NAMESPACES[@]}"; do
        # Assign namespace to shard: (index % total) + 1 == current shard
        ASSIGNED_SHARD=$(( (i % SHARD_TOTAL) + 1 ))
        
        if [ "$ASSIGNED_SHARD" -eq "$SHARD" ]; then
            # Use FullyQualifiedName filter to match namespace prefix
            FILTER_PARTS+=("FullyQualifiedName~${NAMESPACES[$i]}")
        fi
    done
    
    # Join filter parts with OR (|)
    if [ ${#FILTER_PARTS[@]} -gt 0 ]; then
        FILTER=$(IFS="|"; echo "${FILTER_PARTS[*]}")
        echo "Test filter: $FILTER"
        CMD="$CMD --filter \"$FILTER\""
    else
        echo "Warning: No tests assigned to shard $SHARD"
        exit 0
    fi
else
    echo "Running all E2E tests (no sharding)"
fi

# Execute the test command
echo "Executing: $CMD"
eval $CMD
