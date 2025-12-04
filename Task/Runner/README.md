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
| `build-server-publish` | Publish backend for linux-x64 in Release configuration |
| `build-client` | Build the frontend (placeholder) |
| `test-server` | Run backend unit/integration tests |
| `test-server-postman` | Run Postman API tests |
| `test-e2e` | Run E2E Playwright tests (supports sharding with `--shard` and `--shard-total`) |
| `lint-server-verify` | Verify backend formatting & analyzers (no changes). Fails if issues found |
| `lint-server-fix` | Fix backend formatting & analyzer issues automatically |
| `lint-client-verify` | Verify client code formatting and style |
| `lint-client-fix` | Fix client code formatting and style issues automatically |
| `lint-nuke-verify` | Verify Nuke build targets for documentation and naming conventions |
| `lint-nuke-fix` | Fix Nuke build formatting and style issues automatically |
| `run-local-server` | Run backend locally |
| `run-local-client` | Run frontend locally (placeholder) |
| `db-reset` | Reset local SQL Server database by removing docker volume (with confirmation) |
| `db-reset-force` | Reset local SQL Server database without confirmation by removing docker volume |
| `db-migrations-test-apply` | Test EF Core migrations by applying them to a throwaway SQL Server database in Docker (also detects pending model changes via EF Core 9.0) |

### Target Naming Conventions

All Nuke targets follow specific naming conventions:

- **Lint targets**: Must end with either `Verify` (check for issues) or `Fix` (auto-fix issues)
  - `LintServerVerify`, `LintServerFix`, `LintNukeVerify`, `LintNukeFix`
- **Build targets**: Start with `Build` - `BuildServer`, `BuildClient`
- **Test targets**: Start with `Test` - `TestServer`, `TestServerPostman`
- **Database targets**: Start with `Db` or `DbMigrations` - `DbReset`, `DbMigrationsCheckUncommitted`
- **Utility targets**: Use descriptive names - `RunLocalServer`, `InstallClient`, etc.

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

# Run E2E tests with sharding (e.g., shard 1 of 2)
./build.sh test-e2e --shard 1 --shard-total 2

# Reset SQL Server database
./build.sh db-reset-force

# Start SQL Server locally for development
docker compose -f Task/LocalDev/docker-compose.yml up -d sqlserver
```

### Database Reset

The `db-reset` and `db-reset-force` targets reset the SQL Server database used for local development:

**How it works:**
- Detects if the SQL Server docker volume (`localdev_sqlserver-data`) exists
- Stops any running SQL Server containers via docker-compose
- Removes the docker volume completely
- This provides a clean slate - all data and schema are removed

**Usage**:
```bash
# With confirmation prompt
./build.sh db-reset

# Without confirmation (useful for CI/CD)
./build.sh db-reset-force
```

**Note**: To start fresh after reset:
```bash
# Reset will stop containers and remove the volume
./build.sh db-reset-force

# Start SQL Server again with a clean database
docker compose -f Task/LocalDev/docker-compose.yml up -d sqlserver
```

### Windows

```cmd
rem Use build.cmd instead of build.sh
build.cmd show-help
build.cmd build-server
```

### E2E Test Sharding

The E2E tests can be split across multiple shards for parallel execution. This is useful for:
- Speeding up test execution on CI by running tests in parallel
- Identifying flaky tests isolated by shard
- Reducing overall CI time

**How sharding works:**
- Tests are distributed across shards based on their namespace (page-based organization)
- Each shard receives a deterministic subset of test namespaces
- The distribution uses modulo arithmetic: namespace index % total shards determines the assigned shard

**Usage:**
```bash
# Run all tests (no sharding)
./build.sh test-e2e

# Run shard 1 of 2 (ArticlePage, HomePage, ProfilePage, SettingsPage)
./build.sh test-e2e --shard 1 --shard-total 2

# Run shard 2 of 2 (EditorPage, LoginPage, RegisterPage, SwaggerPage)
./build.sh test-e2e --shard 2 --shard-total 2
```

**CI Integration:**
In CI, two separate jobs run each shard in parallel:
- `test-e2e-shard-1`: Runs the first shard
- `test-e2e-shard-2`: Runs the second shard

Both jobs must pass for the PR to be considered successful.

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