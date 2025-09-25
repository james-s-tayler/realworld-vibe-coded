~~---
applyTo: "**/.github/workflows/*.yml"
---~~

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

## Benefits

These principles provide:

- **Better Maintainability**: External scripts are easier to modify, test, and review
- **Cleaner Workflows**: YAML files focus on orchestration rather than business logic  
- **Visibility**: New comments preserve history and show progression over time
- **Debugging**: Separate files make it easier to troubleshoot complex logic
- **Reusability**: External scripts can be used across multiple workflows
- **Testing**: External scripts can be unit tested independently

## File Organization

```
.github/
├── scripts/           # External JavaScript modules
│   ├── data-processor.js
│   ├── comment-generator.js
│   └── utils.js
└── workflows/         # Workflow orchestration only
    ├── ci.yml
    └── deploy.yml
```