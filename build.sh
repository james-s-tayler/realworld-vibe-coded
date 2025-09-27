#!/usr/bin/env bash
set -e

# Define paths
BUILD_PROJECT="build/_build/_build.csproj"
BUILD_OUTPUT="build/_build/bin/Debug/_build.dll"
BUILD_EXECUTABLE="build/_build/bin/Debug/_build"

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
    
    # Check project file
    if [[ "$BUILD_PROJECT" -nt "$output_file" ]]; then
        return 1
    fi
    
    # Check Build.cs and other source files
    if find build/_build -name "*.cs" -newer "$output_file" | grep -q .; then
        return 1
    fi
    
    return 0
}

# Build if necessary
if ! is_build_current; then
    echo "Building Nuke project..."
    dotnet build "$BUILD_PROJECT" --verbosity quiet
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