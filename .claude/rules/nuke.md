## Nuke Build Targets

**Before writing any Nuke code, look up the Nuke docs first.** Nuke has built-in typed APIs for many tools — don't use `ProcessTasks.StartProcess()` when a dedicated API exists.

### Naming Conventions (ArchUnit-enforced)

Target names must start with one of these prefixes:
`Lint`, `Build`, `Test`, `RunLocal`, `Db`, `Install`, `Paths`

Lint targets must end with `Verify` (check-only) or `Fix` (auto-fix).

`LintAllVerify` must `.DependsOn()` every individual `Lint*Verify` target.

### Built-in Tool APIs

Use Nuke's typed APIs instead of raw `ProcessTasks.StartProcess()`:

| Tool | API | Example |
|------|-----|---------|
| npm | `NpmRun()`, `NpmCi()` | `NpmRun(s => s.SetProcessWorkingDirectory(dir).SetCommand("lint"))` |
| Docker | `DockerTasks.Docker()`, `DockerNetworkCreate()`, etc. | `DockerTasks.Docker($"compose -f {file} up -d")` |
| dotnet | `DotNetBuild()`, `DotNetTest()`, `DotNetFormat()` | `DotNetBuild(s => s.SetProjectFile(proj))` |

Only fall back to `ProcessTasks.StartProcess()` for tools without a Nuke API (e.g., `docker compose` subcommands not covered by typed methods).

### File Operations

Use `Nuke.Common.IO` APIs, not `System.IO`:
- Paths: `AbsolutePath` with `/` operator (`RootDirectory / "App" / "Client"`)
- Read: `file.ReadAllLines()`, `file.ReadAllText()`
- Glob: `directory.GlobFiles("**/*.cs")`
- Directories: `directory.CreateOrCleanDirectory()`

### Frontend Tasks

Define scripts in `package.json` and call via `NpmRun()`. Don't invoke `node` directly from Nuke — if a script needs to run during lint/build/test, wire it into the appropriate npm script.

### Every Target Needs a `.Description()`
