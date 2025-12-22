# FlowPilot CLI Potential Improvements

This document outlines potential improvements to the FlowPilot CLI based on lessons learned from the ASP.NET Identity migration retrospective analysis.

## Background

The ASP.NET Identity migration revealed several systematic planning failures that could be mitigated with enhanced CLI tooling:
- Insufficient dependency analysis (handlers using entities not identified upfront)
- Missing ripple effect analysis (changing X requires updating Y, Z, W...)
- Oversized phases (Phase 4 ballooned to 21+ steps)
- Underestimated test maintenance (8 phases of test updates not planned)

## Proposed CLI Enhancements

### 1. Automated Dependency Discovery

**Command:** `flowpilot analyze-dependencies [entity-name]`

**Purpose:** Automatically scan the codebase to identify and document dependencies between code components.

**Features:**
- Use grep/ripgrep to scan the codebase for references to entities, interfaces, or services
- Generate a dependency graph showing:
  - Which handlers/services depend on specific entities
  - Cross-cutting dependencies (logging, auditing, validation)
  - Test dependencies (which tests reference which components)
- Pre-populate the Handler/Service Dependencies table in system-analysis.md
- Output in both human-readable format and structured data (JSON/YAML)

**Example Output:**
```markdown
### Handler/Service Dependencies for User Entity

| Handler/Service | Dependencies | Usage Count | Migration Complexity |
|-----------------|--------------|-------------|---------------------|
| CreateArticleHandler | IRepository<User> | 3 references | High |
| ProfileHandler | IRepository<User> | 5 references | High |
| CommentHandler | IRepository<User> | 2 references | Medium |
```

**Implementation Notes:**
- Use ripgrep for performance with large codebases
- Support multiple languages/frameworks via configurable patterns
- Cache results to avoid repeated scans
- Integrate with `flowpilot next` to auto-run when creating system-analysis.md

**Value:** Would have identified Article/Profile/Comment handlers' dependency on User entity in ASP.NET Identity migration, preventing Phase 4 stuck analysis.

---

### 2. Phase Complexity Analyzer

**Command:** `flowpilot analyze-phase [phase-number]`

**Purpose:** Validate phase scope and complexity, flagging phases that are too large or risky.

**Features:**
- Calculate complexity score based on:
  - Number of implementation steps
  - Number of files affected
  - Handler/service update count
  - Database schema changes
  - Test maintenance required
  - Dependencies on other phases
- Flag phases with >20 steps during `flowpilot lint`
- Suggest natural split points:
  - Separate backend changes from test updates
  - Split by feature area or component
  - Isolate high-risk changes
- Provide warnings for common anti-patterns:
  - Mixing database schema changes with handler updates
  - Combining authentication changes with business logic
  - Bundling multiple independent changes

**Complexity Scoring:**
```
Score = (steps * 2) + (files * 1) + (handlers * 3) + (db_changes * 5) + (test_changes * 2)

Thresholds:
- Low (0-30): Green - proceed with confidence
- Medium (31-60): Yellow - review carefully, consider splitting
- High (61+): Red - should be split into smaller phases
```

**Integration:**
- Run automatically during `flowpilot lint`
- Display warnings in CI/CD pipelines
- Block transitions if complexity exceeds threshold (configurable)

**Value:** Would have flagged Phase 4 (21 steps, high complexity) and suggested splitting it before execution began.

---

### 3. Enhanced Stuck Analysis Workflow

**Command:** `flowpilot stuck --auto-analyze`

**Purpose:** Streamline the stuck analysis process with automated context gathering.

**Features:**
- Auto-generate debug-analysis.md with pre-populated context:
  - Current phase details
  - Recent test failures (parse test output)
  - Recent build errors (parse compiler output)
  - Git diff of current changes
  - Modified files list
  - Uncommitted changes summary
- Parse logs and error output to identify common failure patterns:
  - Transaction/concurrency issues
  - Type mismatches
  - Missing dependencies
  - Configuration errors
- Suggest relevant debugging strategies based on error patterns
- Link to similar historical issues (if stuck analysis exists from previous phases)

**Auto-populated Debug Analysis Sections:**
1. **Problem Definition** - From test failures and error messages
2. **Reproduction** - From test output showing failing scenarios
3. **Recent Changes** - From git diff
4. **Hypotheses** - Generated based on error patterns
5. **Investigation Steps** - Suggested based on problem type

**Integration with Logging:**
- Parse Serilog logs from `Logs/` directory
- Parse Audit.NET logs for transaction issues
- Parse test reports from `Reports/` directory
- Integrate with `./build.sh` output

**Value:** Would have accelerated Phase 4 debugging by automatically identifying transaction coordination issues between UserManager and IRepository.

---

### 4. Template Validation

**Command:** `flowpilot validate-plan`

**Purpose:** Ensure plan quality before execution begins.

**Features:**
- Check research.md:
  - Research checklist completeness (all items checked)
  - Minimum number of references documented (e.g., 5+)
  - Known issues & pitfalls section populated
  - Alternative approaches evaluated
- Check system-analysis.md:
  - Handler/Service Dependencies table populated
  - Ripple effects documented
  - Test infrastructure analyzed
  - Cross-cutting concerns addressed
- Check key-decisions.md:
  - All decision points have selected options
  - High-risk decisions have POC validation noted
  - Rationale provided for each choice
- Check phase-analysis.md:
  - Each phase has explicit scope size (Small/Medium/Large)
  - Ripple effects analyzed per phase
  - No phase marked as Large (>20 steps)
  - Phase validation checklist completed
- Check phase-n-details.md files:
  - Known risks documented
  - Verification targets specified
  - Reality testing guidance included
  - If Phase Fails section completed

**Validation Levels:**
- **Errors** - Block progression (missing required sections)
- **Warnings** - Suggest improvements (incomplete checklists)
- **Info** - Best practice recommendations

**Integration:**
- Run automatically before allowing transition from [phase-analysis] to [phase_1]
- Include in `flowpilot lint`
- Provide detailed validation report with line numbers and specific issues

**Value:** Would have caught missing handler dependency analysis and incomplete research on dual-write transaction semantics before Phase 4 execution.

---

### 5. Progress Tracking & Metrics

**Command:** `flowpilot status --detailed`

**Purpose:** Provide visibility into migration health and progress.

**Features:**
- Track and display key metrics:
  - Phase completion rate (15/17 completed)
  - Actual vs estimated phase duration
  - Stuck count (how many times `flowpilot stuck` was called)
  - Test pass rate over time
  - Files modified per phase
  - Commit velocity
- Generate migration health dashboard:
  - Overall progress percentage
  - Phases ahead/behind schedule
  - High-risk phases upcoming
  - Technical debt accumulated
- Flag anomalies:
  - Phases taking >2x expected duration
  - Multiple stuck analyses on same phase
  - Declining test pass rates
  - Increasing phase complexity
- Export metrics to:
  - Markdown report
  - JSON for integration with dashboards
  - CSV for spreadsheet analysis

**Dashboard Example:**
```
FlowPilot Migration Status: aspnet-identity-migration
════════════════════════════════════════════════════
Progress: [██████████████░░░░░] 15/17 (88%)
Duration: 24 days (est. 21 days) ⚠️ 
Stuck Count: 1 (Phase 4)
Test Pass Rate: 94% ✓
Health: GOOD

Recent Activity:
✓ Phase 15: Completed (2 days ago)
→ Phase 16: In Progress (started 1 day ago)
  Phase 17: Pending

Risks:
⚠️  Phase 16 is 50% over estimated duration
```

**Value:** Would provide early warning when Phase 4 was taking longer than expected, prompting earlier intervention.

---

### 6. Interactive Ripple Effect Analyzer

**Command:** `flowpilot analyze-ripple <entity-name> [--output-format=checklist|graph|table]`

**Purpose:** Automatically identify all code affected by changing a specific entity or interface.

**Features:**
- Find all references to specified entity/interface/service:
  - Direct usages (IRepository<Entity>, Entity parameter types)
  - Indirect usages (DTOs that reference entity, mappers, specifications)
  - Test references (test fixtures, test data, assertions)
- Categorize references by impact:
  - **Breaking changes** - Signature changes, removed properties
  - **Non-breaking changes** - Added properties, new methods
  - **Test updates required** - Test data, assertions, fixtures
- Generate ripple effect checklist for phase-analysis.md:
  - List of files requiring updates
  - List of handlers requiring updates
  - List of tests requiring updates
  - Estimated effort per category
- Visualize dependency graph (optional):
  - Show entity relationships
  - Highlight affected components
  - Identify circular dependencies

**Output Formats:**

**Checklist:**
```markdown
## Ripple Effects for Changing User Entity

### Direct Impact (Must Update):
- [ ] CreateArticleHandler.cs (3 references)
- [ ] ProfileHandler.cs (5 references)
- [ ] CommentHandler.cs (2 references)

### Indirect Impact (Review):
- [ ] UserMapper.cs (mapping logic)
- [ ] UserSpecifications.cs (query specs)
- [ ] ArticleMapper.cs (author mapping)

### Test Impact (Update):
- [ ] ArticlesFixture.cs (test data)
- [ ] ProfilesTests.cs (assertions)
- [ ] AuthTests.cs (user creation)
```

**Graph Output:** ASCII or DOT format for visualization tools

**Integration:**
- Auto-run when system-analysis.md is created
- Cache results for quick re-runs
- Update results when code changes (watch mode)

**Value:** Would have identified all handlers affected by User entity change, preventing the scope explosion in Phase 4.

---

### 7. Test Maintenance Estimator

**Command:** `flowpilot estimate-test-impact [--phase=N]`

**Purpose:** Predict test maintenance effort for planned changes.

**Features:**
- Scan test files to understand test structure:
  - Number of test fixtures
  - Number of test cases
  - Test data creation patterns
  - API contract assertions
- Analyze phase changes for test impact:
  - API endpoint changes → test request/response updates
  - DTO changes → test data updates
  - Authentication changes → test fixture updates
  - Database schema changes → test database updates
- Estimate effort per test category:
  - Unit tests: Quick updates (< 1 hour per file)
  - Integration tests: Medium updates (1-3 hours per file)
  - E2E tests: Slow updates (3-6 hours per file)
  - Postman tests: Medium updates (1-2 hours per collection)
- Suggest creating explicit test-update phases when:
  - >10 test files require updates
  - API contract changes are breaking
  - Multiple test types affected
- Generate test update checklist:
  - List test files requiring updates
  - Categorize by effort (quick/medium/slow)
  - Suggest update order (unit → integration → e2e)

**Example Output:**
```
Test Impact Analysis for Phase 4
═════════════════════════════════

Estimated Test Updates: 45 files
Total Estimated Effort: 18-24 hours

Breakdown:
  Unit Tests:        12 files (~3 hours)
  Integration Tests: 18 files (~9 hours)
  E2E Tests:          8 files (~6 hours)
  Postman Tests:      7 files (~4 hours)

Recommendation: ⚠️  Create separate test-update phase
Reason: >40 test files affected, significant effort required

Suggested Phases:
  Phase 4: Backend changes only
  Phase 5: Update unit and integration tests
  Phase 6: Update E2E and Postman tests
```

**Integration:**
- Run during phase-analysis creation
- Include results in phase-analysis.md
- Flag high test-maintenance phases during `flowpilot lint`

**Value:** Would have identified the need for phases 5-12 (test updates) during initial planning, rather than discovering this during execution.

---

## Implementation Priority

Based on impact and complexity, suggested implementation order:

1. **Phase Complexity Analyzer** (High impact, Medium complexity)
   - Immediately prevents oversized phases
   - Integrates with existing `flowpilot lint`
   - Quick validation with clear criteria

2. **Template Validation** (High impact, Medium complexity)
   - Catches planning gaps before execution
   - Integrates with existing workflow
   - Enforces quality standards

3. **Automated Dependency Discovery** (High impact, High complexity)
   - Addresses root cause of Phase 4 failure
   - Requires robust parsing and analysis
   - Most valuable but most complex

4. **Test Maintenance Estimator** (Medium impact, Medium complexity)
   - Prevents underestimated test effort
   - Relatively straightforward analysis
   - Clear value proposition

5. **Enhanced Stuck Analysis Workflow** (Medium impact, Low complexity)
   - Improves debugging experience
   - Builds on existing stuck command
   - Quick wins for user experience

6. **Interactive Ripple Effect Analyzer** (Medium impact, High complexity)
   - Overlaps with dependency discovery
   - More specialized use case
   - Can build on dependency discovery

7. **Progress Tracking & Metrics** (Low impact, Low complexity)
   - Nice to have for visibility
   - Doesn't prevent failures
   - Good for reporting/dashboards

## Technical Considerations

### Performance
- Use ripgrep for fast code scanning
- Cache dependency graphs to avoid repeated scans
- Run analysis in background where possible
- Provide progress indicators for long-running operations

### Accuracy
- Support multiple languages/frameworks via patterns
- Allow user to verify/correct auto-generated analysis
- Provide confidence scores for auto-detected issues
- Include manual override options

### Integration
- Work seamlessly with existing FlowPilot workflow
- Don't break existing commands or state transitions
- Provide sensible defaults with opt-out options
- Generate output in existing template formats

### User Experience
- Clear, actionable output
- Helpful error messages
- Interactive prompts where appropriate
- Good documentation with examples

## Success Metrics

How to measure if these improvements are effective:

1. **Reduced Stuck Count**: Fewer `flowpilot stuck` invocations per migration
2. **Improved Planning Accuracy**: Actual phases match planned phases (fewer phase-analysis changes)
3. **Better Phase Sizing**: Average phase size within Small/Medium range (<15 steps)
4. **Fewer Surprises**: Test maintenance planned upfront (not discovered during execution)
5. **Faster Completion**: Overall migration duration reduced
6. **Higher Quality**: First-time plan success rate improves

## Future Enhancements

Additional ideas for consideration:

- **AI-Assisted Planning**: Use LLMs to suggest phases based on code analysis
- **Pattern Library**: Common migration patterns (dual-write, strangler fig, etc.)
- **Collaboration Tools**: Multi-user support for team migrations
- **Integration Testing**: Automated testing of phase plans before execution
- **Rollback Automation**: Automated git-based rollback for failed phases
- **Knowledge Base**: Learn from past migrations to improve future plans

## Conclusion

These improvements address the systematic failures identified in the ASP.NET Identity migration retrospective. By automating dependency analysis, validating plan quality, and estimating test maintenance upfront, FlowPilot can help teams create more accurate plans and execute migrations more smoothly with less manual intervention.

The enhancements are designed to be incremental, allowing phased implementation based on priority and complexity. Each improvement builds on FlowPilot's existing strengths while addressing identified weaknesses.
