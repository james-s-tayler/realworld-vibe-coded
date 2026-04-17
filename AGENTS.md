# Agent Instructions

The canonical instructions for any coding agent working in this repo live in
[CLAUDE.md](./CLAUDE.md). Read it in full before making changes — it covers
invariants, build commands (`./build.sh ...`, never `dotnet` directly), code
conventions, protected files, and the rules index under `.claude/rules/`.

This file exists so non-Claude agents (Codex, Cursor, etc.) discover those
instructions via the [AGENTS.md](https://agents.md/) convention. The content
is not duplicated here — edit `CLAUDE.md`, and every agent picks up the change.
