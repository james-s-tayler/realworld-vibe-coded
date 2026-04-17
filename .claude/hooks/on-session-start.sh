#!/bin/bash
cd "$CLAUDE_PROJECT_DIR" || exit 0
./build.sh InstallGitHooks --agent >/dev/null 2>&1 &
exit 0
