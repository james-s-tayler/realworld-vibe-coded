# Cake Build System

This repository includes both **Make** and **Cake** build automation systems with equivalent functionality.

## Quick Start

### Using Cake (Cross-platform)

```bash
# Linux/macOS
./build.sh --target=Help

# Windows
.\build.ps1 -Target Help

# Direct (requires Cake tool to be installed)
dotnet-cake build.cake --target=Help
```

### Using Make (Original)

```bash
make help
```

## Available Tasks

Both build systems provide identical functionality:

| Task | Description |
|------|-------------|
| `Help` | List all available tasks |
| `Build-Server` | Build the .NET backend |
| `Test-Server` | Run backend unit/integration tests |
| `Lint-Server` | Verify backend code formatting |
| `Lint-Make` | Lint the Makefile |
| `Test-Server-Postman` | Run Postman API tests |
| `Run-Local-Server` | Start backend server locally |
| `Db-Reset` | Reset local SQLite database |

## Examples

```bash
# Build the server (Cake)
./build.sh --target=Build-Server

# Run tests (Cake)
./build.sh --target=Test-Server

# Run tests (Make)
make test/server
```

## CI/CD

Both build systems have their own GitHub Actions workflows:

- **Make**: `.github/workflows/ci.yml` (original)
- **Cake**: `.github/workflows/ci-cake.yml` (new)

Both workflows provide identical status checks and functionality.

## Installation

### Cake Tool Installation

The Cake tool is automatically installed by the build scripts. For manual installation:

```bash
dotnet tool install --global Cake.Tool --version 4.0.0
```

### Make Installation

Make is typically pre-installed on Linux/macOS. For Windows, install via:
- Windows Subsystem for Linux (WSL)
- Chocolatey: `choco install make`
- Or use Git Bash