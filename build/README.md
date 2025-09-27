# Nuke Build System

This directory contains the Nuke build system that manages all build, test, and deployment tasks for the project.

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

| Nuke Target | Description |
|-------------|-------------|
| `show-help` | Display available targets |
| `build-server` | Build the .NET backend |
| `build-client` | Build the frontend (placeholder) |
| `test-server` | Run backend unit/integration tests |
| `test-server-postman` | Run Postman API tests |
| `lint-server` | Lint backend code with dotnet format |
| `lint-client` | Lint frontend code (placeholder) |
| `lint-nuke` | Lint Nuke build targets for documentation and naming conventions |
| `run-local-server` | Run backend locally |
| `run-local-client` | Run frontend locally (placeholder) |
| `db-reset` | Reset SQLite database with confirmation |
| `db-reset-force` | Reset SQLite database without confirmation |

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

# Lint Nuke build targets
./build.sh lint-nuke

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

The CI workflow uses Nuke for all build, test, and deployment tasks. The GitHub Actions workflow calls the same Nuke targets that developers use locally, ensuring consistency between local development and CI environments.

## Benefits of Nuke

1. **IDE Support**: Full IntelliSense and debugging support in Visual Studio/VS Code
2. **Type Safety**: Strongly typed build scripts prevent common scripting errors
3. **Cross-Platform**: Native support for Windows, macOS, and Linux
4. **Integration**: Deep integration with .NET ecosystem and tooling
5. **Maintainability**: C# code is easier to maintain than shell scripts
6. **Extensibility**: Easy to add new targets and customize behavior

## Adding New Targets

To add new build targets, modify the `_build/Build.cs` file:

```csharp
Target MyNewTarget => _ => _
    .Description("Description of what this target does")
    .Executes(() =>
    {
        // Implementation here
    });
```

### Target Linting Requirements

All Nuke build targets must follow these requirements:

1. **Documentation**: Every target must include a `.Description()` call explaining what the target does
2. **Naming**: Target names must follow PascalCase convention (e.g., `BuildServer`, `TestServer`)
3. **Validation**: Use `./build.sh lint-nuke` to verify targets comply with requirements

The `lint-nuke` target will validate:
- All targets have proper `.Description()` calls
- Target names follow PascalCase naming conventions
- Build fails if any targets don't meet requirements

This linting is enforced in CI to ensure consistent build target documentation.

For more information about Nuke, visit the [official documentation](https://nuke.build/).