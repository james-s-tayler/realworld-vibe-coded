# CI Pipeline Documentation

## Path-Based Job Gating Strategy

Our CI pipeline uses a path-based job gating strategy where all required checks always appear in GitHub status checks, but jobs only execute when relevant folders change. This approach provides consistent branch protection requirements while optimizing CI resource usage.

## Why We Avoid Paths at the Workflow Level

We **do not** use `paths` or `paths-ignore` at the GitHub Actions workflow trigger level because:

1. **Inconsistent Status Checks**: When paths filters prevent a workflow from running, the required status checks don't appear at all, causing branch protection rules to fail unpredictably.

2. **Branch Protection Complexity**: It's impossible to configure branch protection rules that work reliably when workflows may or may not run based on file changes.

3. **Developer Confusion**: PRs may show different sets of status checks depending on which files changed, making it unclear what requirements must be met.

Instead, our workflow **always runs** but individual jobs are gated based on path changes.

## How the Changes Job Works

The `changes` job is the foundation of our gating strategy:

```yaml
changes:
  name: Detect Changes  
  runs-on: ubuntu-latest
  outputs:
    client: ${{ steps.filter.outputs.client }}
    server: ${{ steps.filter.outputs.server }}
    infra: ${{ steps.filter.outputs.infra }}
    task_runner: ${{ steps.filter.outputs.task_runner }}
  steps:
    - name: Checkout
      uses: actions/checkout@v4
    - name: Check for path changes
      uses: dorny/paths-filter@v3
      id: filter
      with:
        filters: |
          client:
            - 'App/Client/**'
          server:
            - 'App/Server/**'
          infra:
            - 'Infra/**'
          task_runner:
            - 'Task/Runner/**'
```

This job:
- Always runs and completes successfully
- Uses `dorny/paths-filter@v3` to detect changes in specific folders
- Exposes boolean outputs that downstream jobs can reference
- Provides a single source of truth for what changed

## Job Gating with If Conditions

Each downstream job includes:
1. `needs: changes` - Creates dependency on the changes job
2. `if: ${{ needs.changes.outputs.AREA == 'true' }}` - Only runs if relevant changes detected
3. An "Explain skip" step for user-friendly logging when skipped

Example:
```yaml
build-server:
  name: build-server
  runs-on: ubuntu-latest
  needs: changes
  if: ${{ needs.changes.outputs.server == 'true' }}
  steps:
    - name: Explain skip
      if: ${{ needs.changes.outputs.server != 'true' }}
      run: echo "No Server changes detected; skipping server build checks."
    # ... rest of job steps
```

When a job is skipped due to `if` conditions:
- The job shows as "Skipped" in the GitHub UI
- The overall status is treated as successful for branch protection
- The "Explain skip" step provides clear logging about why the job was skipped

## Required Status Checks for Branch Protection

Mark these job names as required in your branch protection settings:

- `changes` - Always runs, ensures change detection works
- `build-nuke` - Nuke build system validation  
- `build-server` - Server application build
- `test-server` - Server unit tests
- `lint-server-verify` - Server code linting
- `lint-nuke-verify` - Nuke build script linting  
- `test-server-postman` - Server integration tests
- `test-e2e` - End-to-end Playwright tests

All jobs will appear in every PR's status checks, but will skip execution when their associated folders haven't changed.

## E2E Test Reports and GitHub Pages Deployment

The `test-e2e` job automatically generates and deploys interactive HTML reports to GitHub Pages:

### Report Generation Process

1. **Test Execution**: Playwright E2E tests run in Docker containers
2. **Trace Collection**: Test traces are saved as ZIP files during execution
3. **HTML Report Generation**: A Node.js script processes test results and traces
4. **GitHub Pages Deployment**: Reports are deployed to `https://username.github.io/repo-name/`

### Report Features

- **Test Results Summary**: Pass/fail counts and execution details
- **Interactive Traces**: Click-to-view traces using Playwright's online trace viewer
- **Responsive Design**: Works on desktop and mobile devices
- **Automatic Updates**: Deployed on every main branch push

### Accessing Reports

**For Main Branch:**
- Live reports are available at the GitHub Pages URL after CI completes
- PR comments include links to the deployed reports

**For Pull Requests:**
- HTML reports are uploaded as artifacts and can be downloaded from the CI run
- Trace files are also available as separate artifacts

### Report Contents

Each report includes:
- Test execution summary (total, passed, failed)
- Links to individual trace files
- Instructions for viewing traces
- Timestamp and execution metadata

The trace files can be viewed using Playwright's trace viewer, which provides:
- Step-by-step test execution timeline
- Screenshots at each action
- Network requests and responses  
- Console logs and page events
- Source code for each test step

### Configuration

Report generation is handled by:
- **Build Target**: `TestE2e` in `Task/Runner/Nuke/Build.cs`
- **Report Script**: `.github/scripts/generate-e2e-report.js`
- **CI Workflow**: `.github/workflows/ci.yml` (test-e2e job)

All jobs will appear in every PR's status checks, but will skip execution when their associated folders haven't changed.

## Extending Filters for New Folders

To add support for new folders:

1. **Add a new filter** in the `changes` job:
   ```yaml
   filters: |
     # ... existing filters
     mobile:
       - 'App/Mobile/**'
   ```

2. **Add a new output** in the `changes` job:
   ```yaml
   outputs:
     # ... existing outputs  
     mobile: ${{ steps.filter.outputs.mobile }}
   ```

3. **Create jobs** that depend on the new filter:
   ```yaml
   build-mobile:
     name: Build (Mobile)
     runs-on: ubuntu-latest
     needs: changes
     if: ${{ needs.changes.outputs.mobile == 'true' }}
     steps:
       - name: Explain skip
         if: ${{ needs.changes.outputs.mobile != 'true' }}
         run: echo "No Mobile changes detected; skipping mobile build."
       # ... build steps
   ```

4. **Update branch protection** to require the new job name.

## Benefits

This approach provides:

- **Predictable Status Checks**: Same checks appear on every PR
- **Resource Optimization**: Jobs only run when relevant code changes
- **Clear Feedback**: Explicit logging when jobs are skipped
- **Simple Branch Protection**: Consistent required status check names
- **Easy Extension**: Simple process to add new folders/jobs