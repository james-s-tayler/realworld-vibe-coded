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

### Worktree Port Isolation

Every listening service in `RunLocal*` targets MUST use `Constants.Worktree.GetPortOffset(RootDirectory)` for its port. This includes Vite, backend, MCP servers — any service that binds a port.

### Vite Environment Variables

Never pass `VITE_`-prefixed env vars from Nuke unless intended for browser-side use. Vite auto-exposes `VITE_*` to client code via `import.meta.env`. Use unprefixed names (e.g., `API_PROXY_TARGET`, `VITE_DEV_PORT` is the exception — it's consumed by vite.config.ts server-side via `process.env`, not `import.meta.env`).

### Every Target Needs a `.Description()`
