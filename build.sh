#!/bin/bash

set -eo pipefail

SCRIPT_DIR=$(cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)
NUKE_ROOT_DIR="$SCRIPT_DIR"
NUKE_EXE="$SCRIPT_DIR/build/bin/Debug/_build.dll"

if [ ! -f "$NUKE_EXE" ]; then
    echo "Building Nuke build project..."
    dotnet build "$SCRIPT_DIR/build/_build.csproj" --nologo -c Debug --verbosity minimal
fi

exec dotnet "$NUKE_EXE" "$@"