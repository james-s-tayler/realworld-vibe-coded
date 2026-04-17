## SharpLens MCP (Roslyn) Usage

SharpLens's Roslyn workspace does not reliably execute source generators. Generator-produced symbols are invisible — treat diagnostics and symbol-resolution as unreliable for any code that depends on them.

### Don't trust for generator-backed code

- **`get_diagnostics`** — phantom `CS0117` / missing-member errors for generator-produced symbols (e.g., `SharedResource.Keys.*` from `Darp.Utils.ResxSourceGenerator`).
- **Symbol resolution** — `find_references`, `find_callers`, `find_implementations`, `go_to_definition` miss generator-produced symbols.
- **`find_unused_code`** — flags DTO/record properties populated by model binding or consumed via pattern matching as unused.

**Authoritative compile check:** run `./build.sh BuildServer`.

### Source generators in this repo

| Project | Generator | Produces |
|---------|-----------|----------|
| `Server.SharedKernel` | `Darp.Utils.ResxSourceGenerator` | `SharedResource.Keys.*` constants from `.resx` files |

### Other tool caveats

- **`get_di_registrations`** — only captures direct generic calls (`AddScoped<T, U>()`, `AddSingleton<T, U>()`, etc.). Extension-method registrations (`AddFastEndpoints()`, `AddIdentity()`, `AddMultiTenant<T>()`, `AddAuthentication()`) are invisible, so most framework wiring is hidden.
- **`get_nuget_dependencies`** — returns `version: "unknown"` under Central Package Management. Read `App/Server/Directory.Packages.props` directly for versions.
- **`semantic_query`** — combined filters behave unexpectedly. Adding `namespaceFilter` returns unrelated symbol kinds (namespaces, classes, properties) even when `isAsync=true` is set. Verify results manually when stacking filters.
- **`get_source_generators`** — all generators report as `Microsoft.CodeAnalysis.IncrementalGeneratorWrapper` with no real type name. If `generatedFiles: []` for a project that has generators, treat related diagnostics as suspect.
- **`find_reflection_usage`** — only catches explicit reflection API calls in source (e.g., `Assembly.GetExecutingAssembly()`). Framework-driven reflection (MediatR/FastEndpoints assembly scanning, Finbuckle tenant resolution) is not flagged.

### Good uses for SharpLens

- Navigation/search of hand-written code — `search_symbols`, `get_type_overview`, `get_file_overview`
- Impact analysis — `find_references`, `find_callers`, `analyze_change_impact`
- Architecture queries — `dependency_graph`, `find_circular_dependencies`, `get_type_hierarchy`, `find_attribute_usages`
- Safe mechanical refactors — `rename_symbol`, `extract_method`, `change_signature`
