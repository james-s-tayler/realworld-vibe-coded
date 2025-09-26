# Nuke Build System Evaluation

This directory contains a Nuke build system implementation that replicates the functionality of the existing Makefile.

## Overview

Nuke is a modern build automation system for .NET projects that provides:
- Cross-platform support (Windows, macOS, Linux)
- Strongly typed build scripts in C#
- Rich IDE support with IntelliSense
- Built-in support for popular tools and workflows

## Structure

```
build/
└── _build/
    ├── _build.csproj    # Nuke build project
    └── Build.cs         # Build targets implementation

.nuke/                   # Nuke configuration directory
├── build.project       # Points to the build project
├── build.schema.json   # Build schema for IDE support
└── parameters.json     # Build parameters

build.sh                 # Cross-platform build script (Unix)
build.cmd                # Cross-platform build script (Windows)
```

## Available Targets

The Nuke build system provides equivalent functionality to all Makefile targets:

| Nuke Target | Makefile Equivalent | Description |
|-------------|-------------------|-------------|
| `show-help` | `help` | Display available targets |
| `build-server` | `build/server` | Build the .NET backend |
| `build-client` | `build/client` | Build the frontend (placeholder) |
| `test-server` | `test/server` | Run backend unit/integration tests |
| `test-server-postman` | `test/server/postman` | Run Postman API tests |
| `lint-server` | `lint/server` | Lint backend code with dotnet format |
| `lint-client` | `lint/client` | Lint frontend code (placeholder) |
| `lint-make` | `lint/make` | Lint Makefile (simplified in Nuke) |
| `run-local-server` | `run-local/server` | Run backend locally |
| `run-local-client` | `run-local/client` | Run frontend locally (placeholder) |
| `db-reset` | `db/reset` | Reset SQLite database with confirmation |
| `db-reset-force` | `db/reset/force` | Reset SQLite database without confirmation |

## Usage

### Local Development

```bash
# Show all available targets
./build.sh show-help

# Build the server
./build.sh build-server

# Run tests
./build.sh test-server

# Lint code
./build.sh lint-server

# Run Postman tests with specific folder
./build.sh test-server-postman --folder Auth

# Reset database
./build.sh db-reset-force
```

### Windows

```cmd
rem Use build.cmd instead of build.sh
build.cmd show-help
build.cmd build-server
```

## CI Integration

A parallel CI workflow (`ci-nuke.yml`) has been created that runs alongside the existing Makefile-based CI (`ci.yml`). Both workflows:

- Run identical test suites
- Generate separate status checks
- Upload artifacts with unique names (e.g., `*-nuke` suffix)
- Create PR comments with build system identification

## Benefits of Nuke

1. **IDE Support**: Full IntelliSense and debugging support in Visual Studio/VS Code
2. **Type Safety**: Strongly typed build scripts prevent common scripting errors
3. **Cross-Platform**: Native support for Windows, macOS, and Linux
4. **Integration**: Deep integration with .NET ecosystem and tooling
5. **Maintainability**: C# code is easier to maintain than shell scripts
6. **Extensibility**: Easy to add new targets and customize behavior

## Limitations Found

1. **Docker Compose**: No built-in Docker Compose support in Nuke, had to use ProcessTasks
2. **Makefile Linting**: The complex Makefile validation logic was simplified in Nuke version
3. **Learning Curve**: Developers need to understand Nuke concepts vs. familiar Make syntax

## Comparison

| Aspect | Make | Nuke |
|--------|------|------|
| **Learning Curve** | Low (familiar syntax) | Medium (C# knowledge required) |
| **IDE Support** | Limited | Excellent |
| **Cross-Platform** | Good (with careful scripting) | Excellent |
| **Debugging** | Difficult | Easy (full debugger support) |
| **Type Safety** | None | Strong |
| **.NET Integration** | Manual | Native |
| **Maintenance** | Complex for advanced scenarios | Easier for complex logic |

## Recommendation

Both build systems have their merits:

- **Keep Make** for teams preferring simple, familiar tooling
- **Consider Nuke** for teams wanting modern tooling with better IDE support and type safety

The parallel CI approach allows gradual evaluation and migration if desired.