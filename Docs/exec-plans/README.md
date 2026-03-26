# Exec Plans

Exec plans are first-class artifacts that define a sequence of stories (work items) with dependencies, test commands, and acceptance criteria.

## Lifecycle

- **`active/`** — Plans currently being worked on. The agent reads the active plan at session start to determine the next story.
- **`completed/`** — Finished plans. Move a plan here when all stories are done and all tests pass.

## Structure

Each plan includes:
- A feature dependency DAG showing what depends on what
- An ordered list of stories, each with: scope, endpoints, dependencies, test command, and done criteria
- An execution summary table for quick reference
- A decision log at the bottom recording implementation decisions made during execution
