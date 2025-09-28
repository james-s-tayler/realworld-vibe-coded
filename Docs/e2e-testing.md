# E2E Testing Documentation

## Overview

This project uses Playwright for .NET with xUnit to run end-to-end tests that verify the complete application functionality. Tests run in Docker containers and generate interactive HTML reports deployed to GitHub Pages.

## Test Architecture

### Components
- **Test Framework**: Playwright for .NET with xUnit
- **Execution Environment**: Docker containers (API + Playwright test runner)
- **Reporting**: Custom HTML reports with interactive trace viewing
- **Deployment**: GitHub Pages for live report hosting

### Docker Setup
The E2E tests use Docker Compose with two services:
1. **API Service**: Runs the .NET backend application
2. **Playwright Service**: Runs the test suite in a containerized environment

## Running E2E Tests

### Local Development
```bash
# Run E2E tests and generate HTML reports
./build.sh test-e2e
```

This command will:
1. Reset the database
2. Build and start Docker containers
3. Execute Playwright tests
4. Generate HTML reports with trace files
5. Clean up containers

### Test Outputs
After running tests, you'll find:
- **Test Results**: `Reports/e2e/e2e-results.trx` (xUnit TRX format)
- **Trace Files**: `Reports/e2e/traces/*.zip` (Playwright trace archives)
- **HTML Report**: `Reports/e2e/html-report/index.html` (Interactive report)

## HTML Reports

### Features
The generated HTML reports include:

- **Test Summary**: Total, passed, and failed test counts
- **Interactive Traces**: Direct links to Playwright's online trace viewer
- **Responsive Design**: Works on all devices
- **Metadata**: Timestamps, file sizes, and execution details

### Viewing Traces
Trace files can be viewed by:
1. Clicking trace links in the HTML report (opens Playwright trace viewer)
2. Downloading ZIP files and viewing locally with `npx playwright show-trace <file.zip>`

### Trace Contents
Each trace includes:
- **Timeline**: Step-by-step execution flow
- **Screenshots**: Visual state at each action
- **Network Activity**: All HTTP requests and responses
- **Console Logs**: Browser console output
- **Source Code**: Test code with execution highlights

## CI/CD Integration

### GitHub Actions
The `test-e2e` job in `.github/workflows/ci.yml`:

1. **Triggers**: Runs when Server, Client, or E2E test files change
2. **Execution**: Uses the same Docker setup as local development
3. **Artifact Upload**: Saves reports and traces as GitHub artifacts
4. **Pages Deployment**: Deploys HTML reports to GitHub Pages (main branch only)
5. **PR Comments**: Includes links to deployed reports in PR comments

### GitHub Pages
- **URL**: `https://<username>.github.io/<repo-name>/`
- **Deployment**: Automatic on main branch pushes
- **Content**: Interactive HTML reports with downloadable traces

## Writing E2E Tests

### Test Structure
Tests are located in `Test/e2e/E2eTests/` and follow this pattern:

```csharp
[Collection("E2E Tests")]
public class MyE2eTests : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions()
        {
            IgnoreHTTPSErrors = true
        };
    }

    [Fact]
    public async Task MyTest_ShouldWork()
    {
        // Start tracing for this test
        await Context.Tracing.StartAsync(new()
        {
            Title = "My Test",
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        try
        {
            // Test implementation
            var baseUrl = Environment.GetEnvironmentVariable("PLAYWRIGHT_BASE_URL") ?? "http://localhost:5000";
            await Page.GotoAsync($"{baseUrl}/my-page");
            
            // Assertions
            await Expect(Page.Locator("h1")).ToHaveTextAsync("Expected Title");
        }
        finally
        {
            // Save trace file
            var tracesDir = "/Reports/e2e/traces";
            Directory.CreateDirectory(tracesDir);
            
            await Context.Tracing.StopAsync(new()
            {
                Path = Path.Combine(tracesDir, $"my_test_trace_{DateTime.Now:yyyyMMdd_HHmmss}.zip")
            });
        }
    }
}
```

### Best Practices

1. **Always use tracing** in the finally block to capture traces even on failure
2. **Use environment variables** for base URLs to work in Docker
3. **Follow naming conventions** for trace files with timestamps
4. **Test realistic user flows** rather than individual UI components
5. **Keep tests independent** - each test should work in isolation

## Troubleshooting

### Common Issues

**Docker containers not starting:**
- Check if ports 5000 is available
- Verify Docker daemon is running
- Run `docker-compose logs` for detailed error messages

**Tests timing out:**
- Increase timeout values in test code
- Check network connectivity between containers
- Verify the API service health check is passing

**Trace files not generated:**
- Ensure `/Reports/e2e/traces` directory exists and is writable
- Check that tracing is started before test execution
- Verify tracing is stopped in the finally block

**HTML report not showing traces:**
- Confirm trace files exist in the traces directory
- Check file permissions on trace files
- Verify the Node.js report generation script completed successfully

### Debug Commands

```bash
# View Docker container logs
docker-compose -f Test/e2e/docker-compose.yml logs

# Run tests with verbose output
./build.sh test-e2e --verbosity detailed

# Generate HTML report manually
node .github/scripts/generate-e2e-report.js

# Test individual components
docker-compose -f Test/e2e/docker-compose.yml up api
```

## Configuration Files

- **Docker Compose**: `Test/e2e/docker-compose.yml`
- **Playwright Dockerfile**: `Test/e2e/Dockerfile.playwright`
- **Test Project**: `Test/e2e/E2eTests/E2eTests.csproj`
- **Report Generator**: `.github/scripts/generate-e2e-report.js`
- **Build Target**: `Task/Runner/Nuke/Build.cs` (TestE2e target)