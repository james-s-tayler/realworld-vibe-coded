# Operations Tooling

This directory contains tooling that facilitates the day-to-day operations of the development team.

## Contents

### TaskRunner (Nuke Build System)

The `TaskRunner/` directory contains the Nuke build system that manages all build, test, and deployment tasks for the project.

**Key Features:**
- Cross-platform build automation
- Strongly typed build scripts in C#
- IDE support with IntelliSense
- Integrated linting, testing, and deployment workflows

**Usage:**
- From repository root: `./nuke.sh [target]` or `./nuke.cmd [target]`
- From TaskRunner directory: `dotnet run --project _build -- [target]`

### Future Operations Tools

This directory is designed to house additional operational tooling such as:

- **Onboarding setup scripts** - Automated environment setup for new team members
- **Diagnostic tools** - Scripts for troubleshooting and system health checks
- **Database utilities** - SQL queries and migration helpers for common maintenance tasks
- **Development tools** - Code generation, scaffolding, and other development aids
- **CI/CD utilities** - Deployment scripts, environment configuration tools
- **Monitoring and alerting tools** - Custom monitoring scripts and alert configuration

## Philosophy

Operations tooling should:
- Reduce manual, error-prone tasks
- Improve developer productivity and onboarding experience
- Maintain consistency across development environments
- Be well-documented and easy to use
- Follow the principle of "infrastructure as code"

## Contributing

When adding new operational tools to this directory:
1. Create a descriptive subdirectory (e.g., `DatabaseUtils/`, `OnboardingScripts/`)
2. Include a README.md explaining the tool's purpose and usage
3. Ensure tools are cross-platform compatible when possible
4. Add appropriate tests and documentation
5. Update this README to reference the new tooling