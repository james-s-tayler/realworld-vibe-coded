---
applyTo: "**/.github/workflows/*.yml"
---

# GitHub Actions Guidelines

## JavaScript Logic in Workflows

### Principle: Externalize Complex JavaScript Logic

Any complex JavaScript logic executed as part of a GitHub Actions step should be externalized to a separate file and not written inline.

**✅ Do:**
- Create external JavaScript files in `.github/scripts/` for complex logic
- Use `require()` to import functions from external modules
- Keep inline JavaScript minimal (simple variable assignments, basic conditionals)
- Include proper error handling and JSDoc documentation in external scripts
- Make external scripts testable and reusable

**❌ Don't:**
- Write complex parsing, processing, or API logic directly in workflow YAML files
- Embed large blocks of JavaScript code inline in `actions/github-script` steps
- Mix business logic with workflow orchestration

### Example:

**Bad:**
```yaml
- name: Process data
  uses: actions/github-script@v7
  with:
    script: |
      const fs = require('fs');
      const data = JSON.parse(fs.readFileSync('data.json'));
      // 50+ lines of complex processing logic...
      const result = complexProcessing(data);
      // More complex logic...
```

**Good:**
```yaml
- name: Process data
  uses: actions/github-script@v7
  with:
    script: |
      const { processData } = require('./.github/scripts/data-processor.js');
      const result = processData('data.json');
```

## PR and Issue Comments

### Principle: Always Create New Comments

Any comments made by a GitHub Actions step should always create a new comment and not update an existing one.

**✅ Do:**
- Always use `github.rest.issues.createComment()` to create fresh comments
- Include timestamps or run identifiers in comments for traceability
- Use footer text like "Generated on each test run" or "Created at ${new Date().toISOString()}"

**❌ Don't:**
- Search for existing comments to update them
- Use `github.rest.issues.updateComment()` in workflows
- Implement "idempotent" commenting that overwrites previous results

### Example:

**Bad:**
```yaml
script: |
  const comments = await github.rest.issues.listComments({...});
  const existingComment = comments.data.find(...);
  if (existingComment) {
    await github.rest.issues.updateComment({...});
  } else {
    await github.rest.issues.createComment({...});
  }
```

**Good:**
```yaml
script: |
  await github.rest.issues.createComment({
    owner: context.repo.owner,
    repo: context.repo.repo,
    issue_number: context.issue.number,
    body: commentBody
  });
```

## CI Status Check Naming

### Principle: Match Kebab-Case NUKE Target Names

All GitHub Actions CI status check names must exactly match the kebab-case name of the corresponding NUKE target they execute. This ensures clarity and consistency between CI check names and build targets.

**✅ Do:**
- Use the exact kebab-case name of the NUKE target for both job ID and job name
- Example: `lint-server-verify` (not `lint-server` or `Lint (Server)`)
- Keep job names simple and automation-friendly

**❌ Don't:**
- Use descriptive names with parentheses like `Build (Server)` or `Lint (Nuke Build)`
- Use abbreviated names that don't match the full NUKE target name
- Mix naming conventions within the same workflow

### Path-Based Job Gating

CI jobs should only run when changes to their respective areas are detected. Use path-based gating to optimize CI resource usage while maintaining predictable status checks.

**✅ Do:**
- Always include a `changes` job that detects path changes using `dorny/paths-filter@v3`
- Gate jobs with `if: ${{ needs.changes.outputs.AREA == 'true' }}`
- Include an "Explain skip" step for user-friendly logging when skipped

**Example:**
```yaml
lint-server-verify:
  name: lint-server-verify
  runs-on: ubuntu-latest
  needs: changes
  if: ${{ needs.changes.outputs.server == 'true' }}
  steps:
    - name: Explain skip
      if: ${{ needs.changes.outputs.server != 'true' }}
      run: echo "No Server changes detected; skipping server linting."
    - name: Run lint
      run: ./build.sh lint-server-verify
```

### Benefits

This approach provides:

- **Automation-Friendly**: Status check names can be programmatically mapped to build targets
- **Clarity**: Developers immediately understand which NUKE target corresponds to which CI check
- **Consistency**: Uniform naming pattern across all CI jobs and build targets
- **Simplicity**: No ambiguity about which command maps to which status check
