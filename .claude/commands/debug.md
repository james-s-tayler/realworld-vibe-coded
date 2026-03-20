You seem to be stuck. Pause all implementation attempts and work through this structured debug analysis to break out of the current local minima.

**Instructions:** Answer each relevant question below. Mark irrelevant sections as N/A. Be honest and specific — vague answers won't help.

---

## 1. Problem Definition & Scope
- What is the exact observable behavior that is incorrect?
- What is the expected behavior, stated concretely?
- What is the smallest, most precise statement of the problem?
- What scope is the failure (single request, user, environment, global)?
- What would definitively prove the problem is fixed?

## 2. Reproduction & Minimization
- Can the problem be reproduced reliably?
- What is the minimal input/scenario that triggers it?
- Can it be reproduced in isolation or a smaller test harness?

## 3. Search Space Reduction
- What single question could eliminate half of remaining hypotheses?
- Where is the earliest point where expected and actual behavior diverge?
- Is the problem in input handling, transformation, or output?
- What invariant appears to be violated?

## 4. Assumptions & Ground Truth
- What assumptions am I making that I have NOT verified?
- What do I know for certain, based on direct observation?
- Am I certain I'm debugging the correct system/environment/version?
- Could I be observing the wrong logs or instance?
- Is it possible I'm solving the wrong problem?

## 5. Observability
- What logs, metrics, or traces are available?
  - Serilog: `Logs/Server.Web/Serilog/`
  - Audit.NET: `Logs/Server.Web/Audit.NET/` (check EntityFrameworkEvent + DatabaseTransactionEvent by CorrelationId)
  - Test reports: `Reports/`
- What additional temporary logging could clarify behavior?
- Are correlation IDs correctly propagated?

## 6. Inputs, Outputs & Boundaries
- Are all inputs exactly what I believe (type, format, encoding)?
- Are there null, empty, default, or boundary values involved?
- Could serialization/deserialization/mapping alter data?
- Is implicit conversion or auto-coercion occurring?

## 7. State, Caching & Persistence
- Could cached/stale data be causing misleading results?
- Have all caches, builds, containers, artifacts been cleared?
- Could previous failures have left corrupted or partial state?

## 8. Configuration & Environment
- What config values influence this behavior?
- Are there environment differences where the issue appears/disappears?
- Are all runtime, library, and platform versions as expected?

## 9. Dependencies & External Systems
- What external services or libraries does this depend on? (EF Core, FastEndpoints, Carbon, etc.)
- Could a dependency be unavailable, degraded, or misbehaving?
- Can the dependency be mocked/stubbed to isolate the issue?

## 10. Control Flow & Dispatch
- Am I certain the code path I'm inspecting is the one executing?
- Is DI, middleware ordering, or dynamic dispatch involved?
- Could an interceptor, override, or fallback be altering behavior?
- Is error handling masking or rewrapping the original failure?

## 11. Cognitive Bias Check
- What belief am I most confident in, and how could it be wrong?
- Am I anchored on a misleading error message or stack frame?
- Am I debugging symptoms instead of root cause?
- Am I avoiding a simple explanation in favor of a clever one?
- If I explained this to someone else, where would my explanation be weakest?

## 12. Hypothesis Testing
- What is my current leading hypothesis?
- What experiment would most strongly confirm or refute it?
- What change would make the bug worse if my hypothesis is correct?

---

After completing the analysis, propose a **concrete next debugging step** based on your findings. Then resume implementation with the new approach.
