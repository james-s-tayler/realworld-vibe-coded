# Conduit — RealWorld Vibe-Coded

## Session Start

Read these files in order:
1. `PROGRESS.md` — context from previous sessions
2. `Docs/exec-plans/active/realworld-spec.md` — story order and dependency DAG
3. `Docs/workflow.md` — mandatory workflow, circuit breaker, context management

Reference as needed:
- `SPEC-REFERENCE.md` — complete API spec
- `.claude/rules/backend.md` — code templates
- `Docs/architecture.md` — tech stack, folder structure, build commands
- `Docs/agentic-engineering-principles/` — first-principles on how to improve agentic-engineering workflows can be derived from the reference material within

## Invariants

These are the primary rules. The Postman test suites are the spec — if a test expects something, it must be true.

1. **All API endpoints must return the exact response shapes defined in SPEC-REFERENCE.md.** The Postman tests validate response shapes — if a test expects a field, that field must exist with the correct type.
2. **All mutating endpoints must validate input and return appropriate error responses on failure.** See SPEC-REFERENCE.md for the error response format.
3. **All authenticated endpoints must return 401 when no valid JWT is present.**
4. **Every feature must have its Postman tests passing before moving to the next feature.** The implementation workflow enforces this.
5. **All compiler warnings and errors must be resolved.** Never suppress or ignore them.
6. **The solution must build cleanly via `./build.sh BuildServer`.** Never run `dotnet` commands directly.
7. **All configurable values must use enums, constants, or reflection.** No magic strings (exception: SQL or UI text).

## Guidance

These are conventions to follow. See `.claude/rules/backend.md` for copy-pasteable code templates.

- Use FastEndpoints with `Endpoint<TRequest, TResponse, TMapper>` pattern
- Use MediatR for CQRS (commands and queries in Server.UseCases)
- Use FluentValidation `Validator<TRequest>` for request validation
- Use `Result<T>` return type from handlers (Ok, NotFound, Invalid, etc.)
- Use `Send.ResultMapperAsync` to map Result to HTTP responses
- Use `ResponseMapper<TResponse, TEntity>` for domain-to-DTO conversion
- Never write XML documentation comments
- Never add comments unless the logic is inherently unclear
- Never use python, perl, awk, sed, or regex for mass refactoring
- Never modify Roslyn analyzers unless explicitly instructed
- If modifying the Nuke build, build it first before committing
