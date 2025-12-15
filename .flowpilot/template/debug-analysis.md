# Debug Analysis

You seem to be stuck on a problem at the moment that your current thinking is unable to resolve. Below is a list of generalised debugging questions to help you generate ideas to break out of your current local minima and get yourself unstuck. Complete the analysis for each question, marking any irrelevant questions as **N/A**.

---

## 1. Problem Definition & Scope

- What is the exact observable behavior that is incorrect?
  - Analysis:
- What is the expected behavior, stated concretely?
  - Analysis:
- What is the smallest, most precise statement of the problem?
  - Analysis:
- What is the scope of the failure (single request, user, machine, environment, or global)?
  - Analysis:
- What would definitively prove that the problem is fixed?
  - Analysis:

---

## 2. Reproduction & Minimization

- Can the problem be reproduced reliably? If not, how often does it occur?
  - Analysis:
- What is the minimal input or scenario that still triggers the problem?
  - Analysis:
- What variables can be removed or simplified without making the problem disappear?
  - Analysis:
- Can the issue be reproduced in isolation or in a smaller test harness?
  - Analysis:
- What is the simplest version of the system where this could still fail?
  - Analysis:

---

## 3. Search Space Reduction

- What single question could eliminate half of the remaining hypotheses?
  - Analysis:
- Where is the earliest point in the system where expected and actual behavior diverge?
  - Analysis:
- Is the problem more likely in input handling, transformation, or output?
  - Analysis:
- Can the system be divided into “before” and “after” this failure?
  - Analysis:
- What invariant appears to be violated?
  - Analysis:

---

## 4. Assumptions & Ground Truth

- What assumptions am I currently making that I have not verified?
  - Analysis:
- What do I know for certain, based on direct observation?
  - Analysis:
- Am I certain I am debugging the correct system, environment, and version?
  - Analysis:
- Could I be observing the wrong logs, metrics, or instance?
  - Analysis:
- Is it possible I am solving the wrong problem?
  - Analysis:

---

## 5. Observability & Instrumentation

- What logs, metrics, or traces are available for this execution?
  - Analysis:
- What additional temporary logging or tracing could clarify behavior?
  - Analysis:
- Are correlation IDs or request IDs available and correctly propagated?
  - Analysis:
- What key inputs, outputs, or state transitions should be logged but currently are not?
  - Analysis:
- Can I assert or validate invariants at system boundaries?
  - Analysis:

---

## 6. Inputs, Outputs & Boundaries

- Are all inputs exactly what I believe them to be (type, format, encoding, units)?
  - Analysis:
- Are there null, empty, default, or boundary values involved?
  - Analysis:
- Are there serialization, deserialization, or mapping steps that could alter data?
  - Analysis:
- Could this be an off-by-one, rounding, or precision error?
  - Analysis:
- Is implicit conversion or auto-coercion occurring?
  - Analysis:

---

## 7. State, Caching & Persistence

- What state persists across executions that could affect behavior?
  - Analysis:
- Could cached data be causing stale or misleading results?
  - Analysis:
- Have all relevant caches, builds, containers, or artifacts been cleared?
  - Analysis:
- Is the system truly starting from a clean baseline?
  - Analysis:
- Could previous failures have left corrupted or partial state behind?
  - Analysis:

---

## 8. Time, Ordering & Concurrency

- Does behavior change when concurrency or parallelism is reduced?
  - Analysis:
- Are there ordering assumptions that may not always hold?
  - Analysis:
- Could this be a race condition, deadlock, or timing-sensitive issue?
  - Analysis:
- Are retries, timeouts, or asynchronous processing involved?
  - Analysis:
- Is eventual consistency a factor?
  - Analysis:

---

## 9. Configuration & Environment

- What configuration values influence this behavior?
  - Analysis:
- Could configuration precedence or overrides be affecting execution?
  - Analysis:
- Are there differences between environments where the issue appears or disappears?
  - Analysis:
- Are all runtime, library, and platform versions what I expect?
  - Analysis:
- Does the issue occur on all machines or only specific ones?
  - Analysis:

---

## 10. Dependencies & External Systems

- What external services or libraries does this depend on?
  - Analysis:
- Could an external dependency be unavailable, degraded, or misbehaving?
  - Analysis:
- Can the dependency be mocked, stubbed, or bypassed to isolate the issue?
  - Analysis:
- Are failure modes (timeouts, partial responses) handled correctly?
  - Analysis:
- Could a dependency have changed without a corresponding code change?
  - Analysis:

---

## 11. Data Integrity & Invariants

- What invariants should always hold true?
  - Analysis:
- Is the data complete, valid, and from a single coherent version?
  - Analysis:
- Could there be a poison record or malformed data triggering the failure?
  - Analysis:
- Is there a schema mismatch or unexpected nullability?
  - Analysis:
- Are constraints being silently violated?
  - Analysis:

---

## 12. Control Flow & Dispatch

- Am I certain the code path I’m inspecting is the one executing?
  - Analysis:
- Is dynamic dispatch, dependency injection, or middleware ordering involved?
  - Analysis:
- Could an override, interceptor, or fallback handler be altering behavior?
  - Analysis:
- Is error handling masking or rewrapping the original failure?
  - Analysis:
- Are guard clauses or early returns skipping logic?
  - Analysis:

---

## 13. Build, Packaging & Deployment

- Was the system rebuilt after the last change?
  - Analysis:
- Was the correct artifact deployed?
  - Analysis:
- Are there multiple versions of the system running simultaneously?
  - Analysis:
- Could the client and server be out of sync?
  - Analysis:
- Are symbols, source maps, or debug info aligned with the running code?
  - Analysis:

---

## 14. Security, Permissions & Identity

- Could this be an authentication or authorization issue?
  - Analysis:
- Does behavior differ when running as a different user or role?
  - Analysis:
- Are locks, leases, or ownership semantics involved?
  - Analysis:
- Are credentials, certificates, or secrets valid and current?
  - Analysis:
- Is the system correctly rejecting an operation that violates policy?
  - Analysis:

---

## 15. Cognitive Bias & Debugging Traps

- What belief am I most confident in, and how could it be wrong?
  - Analysis:
- Am I anchored on a misleading error message or stack frame?
  - Analysis:
- Am I debugging symptoms instead of root cause?
  - Analysis:
- Am I avoiding a simple explanation in favor of a clever one?
  - Analysis:
- If I explained this to someone else, where would my explanation be weakest?
  - Analysis:

---

## 16. Comparison & History

- Is there a known-good version or example to compare against?
  - Analysis:
- What differences exist between working and failing cases?
  - Analysis:
- Do version control history or configuration changes correlate with the failure?
  - Analysis:
- Can I bisect commits, config, or dependency versions to find the introduction point?
  - Analysis:
- Has anything external changed recently that could affect this system?
  - Analysis:

---

## 17. Hypothesis Testing

- What is my current leading hypothesis?
  - Analysis:
- What experiment would most strongly confirm or refute it?
  - Analysis:
- What change would make the bug worse if my hypothesis is correct?
  - Analysis:
- What change would mask the bug without fixing it?
  - Analysis:
- What simpler model explains all observed behavior?
  - Analysis:

---

## 18. Resolution & Prevention

- What specific change will resolve the issue?
  - Analysis:
- What test would prevent this regression in the future?
  - Analysis:
- What monitoring or alert would detect this earlier?
  - Analysis:
- What invariant or constraint could be enforced to make this class of bug impossible?
  - Analysis:
- What documentation or tooling change would reduce recurrence?
  - Analysis:

---
