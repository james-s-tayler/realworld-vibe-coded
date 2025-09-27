#!/usr/bin/env bash
set -e

# Define paths
BUILD_SOLUTION="Ops/TaskRunner/Nuke.sln"
BUILD_OUTPUT="Ops/TaskRunner/Nuke/bin/Debug/Nuke.dll"
BUILD_EXECUTABLE="Ops/TaskRunner/Nuke/bin/Debug/Nuke"

# Function to check if build is up to date
is_build_current() {
    # Check if output exists
    if [[ ! -f "$BUILD_OUTPUT" ]] && [[ ! -f "$BUILD_EXECUTABLE" ]]; then
        return 1
    fi
    
    # Check if any source files are newer than the output
    local output_file="$BUILD_OUTPUT"
    if [[ -f "$BUILD_EXECUTABLE" ]]; then
        output_file="$BUILD_EXECUTABLE"
    fi
    
    # Check solution file and project files
    if [[ "$BUILD_SOLUTION" -nt "$output_file" ]]; then
        return 1
    fi
    
    # Check individual project files
    if find Ops/TaskRunner -name "*.csproj" -newer "$output_file" | grep -q .; then
        return 1
    fi
    
    # Check Build.cs and other source files
    if find Ops/TaskRunner/Nuke -name "*.cs" -newer "$output_file" | grep -q .; then
        return 1
    fi
    
    return 0
}

# Build if necessary
if ! is_build_current; then
    echo "Building Nuke solution..."
    dotnet build "$BUILD_SOLUTION" --verbosity quiet
fi

# Run the built executable
if [[ -f "$BUILD_EXECUTABLE" ]]; then
    exec "$BUILD_EXECUTABLE" "$@"
elif [[ -f "$BUILD_OUTPUT" ]]; then
    exec dotnet "$BUILD_OUTPUT" "$@"
else
    echo "Error: Could not find built Nuke executable"
    exit 1
fi