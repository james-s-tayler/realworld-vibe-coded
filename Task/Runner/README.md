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
Task/Runner/
└── Nuke/
    ├── Nuke.csproj    # Nuke build project
    └── Build.cs         # Build targets implementation

.nuke/                   # Nuke configuration directory
├── build.project       # Points to the build project
├── build.schema.json   # Build schema for IDE support
└── parameters.json     # Build parameters

build.sh                  # Cross-platform build script (Unix)
build.cmd                 # Cross-platform build script (Windows)
```

## Available Targets

| Nuke Target | Description |
|-------------|-------------|
| `build-server` | Build the .NET backend |
| `build-client` | Build the frontend (placeholder) |
| `test-server` | Run backend unit/integration tests |
| `test-server-postman` | Run Postman API tests |
| `test-server-e2e` | Run E2E Playwright tests |
| `lint-server-verify` | Verify backend formatting & analyzers (no changes). Fails if issues found |
| `lint-server-fix` | Fix backend formatting & analyzer issues automatically |
| `lint-client-verify` | Verify client code formatting and style |
| `lint-client-fix` | Fix client code formatting and style issues automatically |
| `lint-nuke-verify` | Verify Nuke build targets for documentation and naming conventions |
| `lint-nuke-fix` | Fix Nuke build formatting and style issues automatically |
| `run-local-server` | Run backend locally |
| `run-local-client` | Run frontend locally (placeholder) |
| `db-reset` | Reset SQLite database with confirmation |
| `db-reset-force` | Reset SQLite database without confirmation |

### Target Naming Conventions

All Nuke targets follow specific naming conventions:

- **Lint targets**: Must end with either `Verify` (check for issues) or `Fix` (auto-fix issues)
  - `LintServerVerify`, `LintServerFix`, `LintNukeVerify`, `LintNukeFix`
- **Build targets**: Start with `Build` - `BuildServer`, `BuildClient`
- **Test targets**: Start with `Test` - `TestServer`, `TestServerPostman`
- **Utility targets**: Use descriptive names - `RunLocalServer`, `DbReset`, etc.

These conventions are enforced by ArchUnit.NET tests in the `lint-nuke-verify` target.

## Usage

### Local Development

```bash
# Show all available targets
./build.sh --help

# Build the server
./build.sh build-server

# Run tests
./build.sh test-server

# Verify code formatting (fails if issues found)
./build.sh lint-server-verify

# Fix code formatting issues automatically
./build.sh lint-server-fix

# Verify Nuke build targets compliance
./build.sh lint-nuke-verify

# Fix Nuke build formatting issues
./build.sh lint-nuke-fix

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

To add new build targets, modify the `Nuke/Build.cs` file:

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
3. **Lint naming**: Targets starting with "Lint" must end with either "Verify" or "Fix"
4. **Validation**: Use `./build.sh lint-nuke-verify` to verify targets comply with requirements

The linting system includes:
- **ArchUnit.NET**: Validates target architecture and naming conventions at test time
- **Custom Roslyn Analyzer**: Enforces `.Description()` calls at compile time
- **dotnet format**: Ensures consistent code formatting

Use the "Fix" targets to automatically resolve formatting issues:
- `./build.sh lint-nuke-fix` - Fix Nuke build formatting
- `./build.sh lint-server-fix` - Fix server code formatting

This linting is enforced in CI to ensure consistent build target documentation and formatting.

For more information about Nuke, visit the [official documentation](https://nuke.build/).