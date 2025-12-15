# Debug Analysis: Test Execution Hang

Based on the debug analysis template, investigating why tests hang with the new SQL Server assembly fixture.

---

## 1. Problem Definition & Scope

- **What is the exact observable behavior that is incorrect?**
  - Analysis: Tests execute but never complete/report results. Process hangs indefinitely during test execution. No test output is produced.

- **What is the expected behavior, stated concretely?**
  - Analysis: Tests should execute, complete, and report PASS/FAIL results within reasonable time (~30-60s for single test).

- **What is the smallest, most precise statement of the problem?**
  - Analysis: xUnit test runner hangs during fixture initialization or test execution when using `SqlServerAssemblyFixture` with FastEndpoints `AppFixture<T>`.

- **What is the scope of the failure?**
  - Analysis: Affects all tests in collections using the new assembly fixture. Single test or multiple tests - both hang.

- **What would definitively prove that the problem is fixed?**
  - Analysis: Single test executing to completion with PASS result, showing test output and cleanup.

---

## 2. Reproduction & Minimization

- **Can the problem be reproduced reliably?**
  - Analysis: Yes, 100% reproducible. Every attempt to run tests with new fixture hangs.

- **What is the minimal input or scenario that still triggers the problem?**
  - Analysis: Single test method in UsersTests class with SqlServer Assembly Collection attribute.

- **What variables can be removed or simplified?**
  - Analysis: Could try removing backup/restore logic and just use direct DB creation to isolate if it's the SQL operations or the fixture lifecycle.

- **Can the issue be reproduced in isolation or in a smaller test harness?**
  - Analysis: Should create minimal reproduction: basic xUnit collection fixture + FastEndpoints AppFixture without SQL operations.

- **What is the simplest version where this could still fail?**
  - Analysis: Empty assembly fixture that implements IAsyncLifetime + empty test class using FastEndpoints TestBase.

---

## 3. Search Space Reduction

- **What single question could eliminate half of the remaining hypotheses?**
  - Analysis: Does a minimal xUnit collection fixture work with FastEndpoints AppFixture<T> WAF caching?

- **Where is the earliest point where expected and actual behavior diverge?**
  - Analysis: Likely during fixture initialization - either `SqlServerAssemblyFixture.InitializeAsync()` or `UsersFixture.PreSetupAsync()`.

- **Is the problem more likely in input handling, transformation, or output?**
  - Analysis: Problem is in the initialization/setup phase, not in test execution or output.

- **Can the system be divided into "before" and "after" this failure?**
  - Analysis: Before: old fixtures work fine. After: new assembly-level fixture causes hang.

- **What invariant appears to be violated?**
  - Analysis: xUnit expects fixture initialization to complete and return control. Something is blocking.

---

## 4. Assumptions & Ground Truth

- **What assumptions am I currently making that I have not verified?**
  - Analysis:
    1. Assumption: FastEndpoints AppFixture<T> is compatible with xUnit v3 collection fixtures
    2. Assumption: Assembly-level fixtures can inject into collection-level fixtures
    3. Assumption: The SQL Server container operations complete successfully
    4. Assumption: xUnit fixture lifecycle methods are being called in expected order

- **What do I know for certain, based on direct observation?**
  - Analysis:
    1. Container starts successfully (verified by backup file creation)
    2. Backup file is created (548KB at /tmp/sqlserver-backups/template.bak)
    3. Schema version file is created
    4. Build succeeds with no compilation errors
    5. E2E tests in pre-commit hooks pass (using old fixtures)

- **Am I certain I am debugging the correct system, environment, and version?**
  - Analysis: Yes, working in correct repo, correct branch with latest changes.

- **Could I be observing the wrong logs, metrics, or instance?**
  - Analysis: No visible test output - that's the problem. Need to add explicit logging to fixture methods.

- **Is it possible I am solving the wrong problem?**
  - Analysis: Possibly. The real problem might be architectural incompatibility between xUnit collection fixtures and FastEndpoints WAF caching.

---

## 5. Observability & Instrumentation

- **What logs, metrics, or traces are available?**
  - Analysis: Console.WriteLine in fixture shows container start and backup creation. Nothing after that point.

- **What additional temporary logging or tracing could clarify behavior?**
  - Analysis:
    1. Add Console.WriteLine at start/end of every fixture method
    2. Add logging in UsersFixture.PreSetupAsync/SetupAsync
    3. Add logging in FastEndpoints base class methods
    4. Check if tests are waiting for input

- **Are correlation IDs or request IDs available?**
  - Analysis: N/A for test execution.

- **What key inputs, outputs, or state transitions should be logged but currently are not?**
  - Analysis:
    1. Entry/exit of SqlServerAssemblyFixture.InitializeAsync
    2. Entry/exit of UsersFixture.PreSetupAsync
    3. Entry/exit of UsersFixture.SetupAsync
    4. Database lease acquire/release
    5. WAF initialization state

- **Can I assert or validate invariants at system boundaries?**
  - Analysis: Could add assertions that fixture initialization completed, database was leased, etc.

---

## 6. Inputs, Outputs & Boundaries

- **Are all inputs exactly what I believe them to be?**
  - Analysis: N/A - no external inputs, all internal fixture initialization.

- **Are there null, empty, default, or boundary values involved?**
  - Analysis: Constructor injection of `SqlServerAssemblyFixture` into `UsersFixture` - verify it's not null.

- **Are there serialization, deserialization, or mapping steps?**
  - Analysis: No.

- **Could this be an off-by-one, rounding, or precision error?**
  - Analysis: No.

- **Is implicit conversion or auto-coercion occurring?**
  - Analysis: No.

---

## 7. State, Caching & Persistence

- **What state persists across executions?**
  - Analysis:
    1. Backup files in /tmp/sqlserver-backups/ persist
    2. Docker containers persist (Testcontainers should clean up)
    3. FastEndpoints WAF caching - **THIS IS KEY**

- **Could cached data be causing stale or misleading results?**
  - Analysis: FastEndpoints WAF caching could be expecting singleton fixture lifecycle but getting assembly fixture lifecycle.

- **Have all relevant caches, builds, containers, or artifacts been cleared?**
  - Analysis: Need to verify Docker containers are cleaned up. Backup files persisting is expected.

- **Is the system truly starting from a clean baseline?**
  - Analysis: Each test run should be clean except for backup files (which is desired).

- **Could previous failures have left corrupted or partial state behind?**
  - Analysis: Possible dangling containers or connections. Should verify with `docker ps`.

---

## 8. Time, Ordering & Concurrency

- **Does behavior change when concurrency or parallelism is reduced?**
  - Analysis: Tests already configured with `parallelizeAssembly: false, parallelizeTestCollections: false`.

- **Are there ordering assumptions that may not always hold?**
  - Analysis: **CRITICAL**: Fixture initialization order:
    1. xUnit creates SqlServerAssemblyFixture (assembly level)
    2. xUnit creates UsersFixture (collection level) with SqlServerAssemblyFixture injected
    3. FastEndpoints AppFixture<T> initializes WAF
    4. Problem: WAF might expect fixture to be singleton per collection, not shared across collections

- **Could this be a race condition, deadlock, or timing-sensitive issue?**
  - Analysis: Possible deadlock. WAF initialization might be waiting for something that never completes.

- **Are retries, timeouts, or asynchronous processing involved?**
  - Analysis: All async/await operations. Could be an incomplete async chain or forgotten await.

- **Is eventual consistency a factor?**
  - Analysis: No.

---

## 9. Configuration & Environment

- **What configuration values influence this behavior?**
  - Analysis: xunit.runner.json has parallelization disabled.

- **Could configuration precedence or overrides be affecting execution?**
  - Analysis: No evidence of this.

- **Are there differences between environments?**
  - Analysis: Running in GitHub Actions Linux environment. Should be consistent.

- **Are all runtime, library, and platform versions what I expect?**
  - Analysis: xUnit v3.1.0, FastEndpoints 7.0.1, .NET 9.0.

- **Does the issue occur on all machines or only specific ones?**
  - Analysis: Unknown - only tested in CI environment.

---

## 10. Dependencies & External Systems

- **What external services or libraries does this depend on?**
  - Analysis:
    1. Docker for Testcontainers
    2. SQL Server container image
    3. FastEndpoints.Testing library
    4. xUnit v3

- **Could an external dependency be unavailable, degraded, or misbehaving?**
  - Analysis: Docker and SQL Server work (backup created successfully).

- **Can the dependency be mocked, stubbed, or bypassed?**
  - Analysis: Could stub out SQL operations to test if it's xUnit+FastEndpoints compatibility.

- **Are failure modes handled correctly?**
  - Analysis: Exception handling exists but might not cover all cases.

- **Could a dependency have changed?**
  - Analysis: All dependencies are version-pinned in Directory.Packages.props.

---

## 11. Data Integrity & Invariants

- **What invariants should always hold true?**
  - Analysis:
    1. Assembly fixture should initialize exactly once
    2. Collection fixture should initialize once per collection
    3. Each test should get a unique database
    4. Database should be dropped after test

- **Is the data complete, valid, and from a single coherent version?**
  - Analysis: Backup file successfully created, suggests SQL operations work.

- **Could there be a poison record or malformed data?**
  - Analysis: No data involved yet - problem is in initialization.

- **Is there a schema mismatch or unexpected nullability?**
  - Analysis: No.

- **Are constraints being silently violated?**
  - Analysis: Possibly fixture lifecycle constraints.

---

## 12. Control Flow & Dispatch

- **Am I certain the code path I'm inspecting is the one executing?**
  - Analysis: Need to add logging to verify control flow.

- **Is dynamic dispatch, dependency injection, or middleware ordering involved?**
  - Analysis: **YES - CRITICAL**: 
    1. xUnit dependency injection for fixtures
    2. FastEndpoints DI for WAF
    3. These might conflict or have incompatible lifecycle expectations

- **Could an override, interceptor, or fallback handler be altering behavior?**
  - Analysis: FastEndpoints might have initialization hooks that are blocking.

- **Is error handling masking or rewrapping the original failure?**
  - Analysis: Possible. Exceptions might be swallowed in async initialization.

- **Are guard clauses or early returns skipping logic?**
  - Analysis: No evidence of this.

---

## 13. Build, Packaging & Deployment

- **Was the system rebuilt after the last change?**
  - Analysis: Yes, build succeeds.

- **Was the correct artifact deployed?**
  - Analysis: Yes, running latest built artifacts.

- **Are there multiple versions running simultaneously?**
  - Analysis: No.

- **Could the client and server be out of sync?**
  - Analysis: N/A.

- **Are symbols, source maps, or debug info aligned?**
  - Analysis: Yes, debug build with symbols.

---

## 14. Security, Permissions & Identity

- **Could this be an authentication or authorization issue?**
  - Analysis: No.

- **Does behavior differ when running as a different user or role?**
  - Analysis: N/A.

- **Are locks, leases, or ownership semantics involved?**
  - Analysis: SemaphoreSlim used for backup creation - could be deadlocking if not released properly.

- **Are credentials, certificates, or secrets valid and current?**
  - Analysis: SQL Server uses default SA password, works fine.

- **Is the system correctly rejecting an operation?**
  - Analysis: No rejection, just hanging.

---

## 15. Cognitive Bias & Debugging Traps

- **What belief am I most confident in, and how could it be wrong?**
  - Analysis: Belief: "The fixture infrastructure is correct." Could be wrong: The entire approach of assembly-level fixture + per-test restoration might be incompatible with FastEndpoints WAF caching.

- **Am I anchored on a misleading error message or stack frame?**
  - Analysis: No error message - that's the problem. Hanging without error.

- **Am I debugging symptoms instead of root cause?**
  - Analysis: Possibly. The hang might be a symptom of architectural incompatibility.

- **Am I avoiding a simple explanation in favor of a clever one?**
  - Analysis: Simple explanation: FastEndpoints AppFixture<T> expects each fixture to be independent, not nested/injected.

- **If I explained this to someone else, where would my explanation be weakest?**
  - Analysis: "I assume xUnit collection fixtures can inject assembly fixtures and FastEndpoints will work with that." This is unverified.

---

## 16. Comparison & History

- **Is there a known-good version or example to compare against?**
  - Analysis: Yes - old fixtures work fine. They don't use assembly-level injection.

- **What differences exist between working and failing cases?**
  - Analysis:
    - Working: Each collection fixture manages its own container
    - Failing: Assembly fixture manages container, collection fixture injects it

- **Do version control history or configuration changes correlate?**
  - Analysis: Problem introduced with new assembly fixture approach.

- **Can I bisect commits, config, or dependency versions?**
  - Analysis: Not needed - clear before/after point.

- **Has anything external changed recently?**
  - Analysis: No.

---

## 17. Hypothesis Testing

- **What is my current leading hypothesis?**
  - Analysis: **FastEndpoints AppFixture<T> WAF caching is incompatible with xUnit v3 collection fixtures that depend on assembly-level fixtures via constructor injection.**

- **What experiment would most strongly confirm or refute it?**
  - Analysis:
    1. Create minimal test: xUnit collection fixture with constructor parameter (not SQL-related)
    2. Inherit from FastEndpoints TestBase
    3. See if it hangs
    
    If it hangs: Architectural incompatibility confirmed
    If it works: Problem is specific to SQL operations

- **What change would make the bug worse if my hypothesis is correct?**
  - Analysis: Adding more complexity to fixture initialization.

- **What change would mask the bug without fixing it?**
  - Analysis: Reverting to per-collection containers (original approach).

- **What simpler model explains all observed behavior?**
  - Analysis: FastEndpoints AppFixture expects to fully control fixture lifecycle and doesn't support dependency injection from parent fixtures.

---

## 18. Resolution & Prevention

- **What specific change will resolve the issue?**
  - Analysis: **Two possible approaches:**
    1. **Pragmatic**: Revert to per-collection containers but ensure they share the same SQL Server instance (single container, multiple databases)
    2. **Ideal** (if feasible): Modify fixture pattern to not use constructor injection - use static/singleton pattern instead

- **What test would prevent this regression?**
  - Analysis: Integration test that verifies fixture initialization completes within timeout.

- **What monitoring or alert would detect this earlier?**
  - Analysis: Test timeout in CI (already exists).

- **What invariant or constraint could be enforced?**
  - Analysis: Fixture initialization must complete within 30 seconds.

- **What documentation or tooling change would reduce recurrence?**
  - Analysis: Document FastEndpoints AppFixture compatibility constraints.

---

## Recommended Next Steps

1. **Immediate**: Create minimal reproduction to confirm hypothesis
2. **If confirmed**: Implement pragmatic solution (shared container, per-collection databases)
3. **Validate**: Run full test suite
4. **Document**: Add notes about FastEndpoints fixture compatibility
