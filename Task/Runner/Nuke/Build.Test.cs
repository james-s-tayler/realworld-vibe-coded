using System.Text.Json;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Tools.ReportGenerator;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;
using static Nuke.Common.Tools.ReportGenerator.ReportGeneratorTasks;

public partial class Build
{
  [Parameter("Toggle special behavior for CI environment")]
  internal readonly bool SkipPublish;

  [Parameter("Stop on first test failure (for Postman tests)")]
  internal readonly bool Bail;

  internal Target TestServer => _ => _
      .Description("Run backend tests and generate test and coverage reports")
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        // Get all test projects in the solution
        var testsDirectory = RootDirectory / "App" / "Server" / "tests";
        var testProjects = testsDirectory.GlobDirectories("*")
              .Select(dir => dir / $"{dir.Name}.csproj")
              .Where(project => project.FileExists())
              .ToArray();

        var failures = new List<string>();

        // Run tests for each project with unique result files
        foreach (var testProject in testProjects)
        {
          var projectName = testProject.NameWithoutExtension;
          var logFileName = $"{projectName}-results.trx";

          Log.Information("Running tests for {ProjectName}", projectName);

          try
          {
            DotNetTest(s => s
                  .SetProjectFile(testProject)
                  .SetLoggers($"trx;LogFileName={logFileName}")
                  .SetResultsDirectory(ReportsServerResultsDirectory)
                  .SetSettingsFile(RootDirectory / "App" / "Server" / "coverlet.runsettings")
                  .AddProcessAdditionalArguments("--collect:\"XPlat Code Coverage\""));
          }
          catch (ProcessException)
          {
            failures.Add(testProject.Name);
          }
        }

        var reportFile = ReportsServerArtifactsDirectory / "Tests" / "Report.md";

        Liquid($"--inputs \"File=*.trx;Folder={ReportsServerResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestServer)} Results\"");

        // Extract summary from Report.md (everything before first "---")
        var reportSummaryFile = ReportsServerArtifactsDirectory / "Tests" / "ReportSummary.md";
        ExtractReportSummary(reportFile, reportSummaryFile);

        ReportGenerator(s => s
              .SetReports(ReportsServerResultsDirectory / "**" / "coverage.cobertura.xml")
              .SetTargetDirectory(ReportsServerArtifactsDirectory / "Coverage")
              .SetReportTypes(ReportTypes.Html, ReportTypes.Cobertura));

        if (failures.Any())
        {
          var failedProjects = string.Join(", ", failures);
          throw new Exception($"Some test projects failed: {failedProjects}");
        }
      });

  internal Target TestClient => _ => _
      .Description("Run client tests")
      .DependsOn(InstallClient)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        Log.Information("Running client tests in {ClientDirectory}", ClientDirectory);

        var testsFailed = false;
        try
        {
          NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("test:run")
                .SetArguments("--coverage"));
        }
        catch (ProcessException)
        {
          testsFailed = true;
        }

        // Generate LiquidTestReport from TRX files
        var reportFile = ReportsClientArtifactsDirectory / "Report.md";

        try
        {
          Liquid($"--inputs \"File=*.trx;Folder={ReportsClientResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestClient)} Results\"");

          // Extract summary from Report.md (everything before first "---")
          var reportSummaryFile = ReportsClientArtifactsDirectory / "ReportSummary.md";
          ExtractReportSummary(reportFile, reportSummaryFile);
        }
        catch (Exception ex)
        {
          Log.Warning("Failed to generate LiquidTestReport: {Message}", ex.Message);
        }

        if (testsFailed)
        {
          throw new Exception("Client tests failed");
        }
      });

  internal Target TestServerPostmanArticlesEmpty => _ => _
      .Description("Run postman tests for ArticlesEmpty collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        RunPostmanCollection("ArticlesEmpty");
      });

  internal Target TestServerPostmanAuth => _ => _
      .Description("Run postman tests for Auth collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        RunPostmanCollection("Auth");
      });

  internal Target TestServerPostmanProfiles => _ => _
      .Description("Run postman tests for Profiles collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        RunPostmanCollection("Profiles");
      });

  internal Target TestServerPostmanFeedAndArticles => _ => _
      .Description("Run postman tests for FeedAndArticles collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        RunPostmanCollection("FeedAndArticles");
      });

  internal Target TestServerPostmanArticle => _ => _
      .Description("Run postman tests for Article collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        RunPostmanCollection("Article");
      });

  internal Target TestE2e => _ =>
  {
    _.Description("Run E2E Playwright tests using Docker Compose")
    .DependsOn(BuildServerPublish)
    .DependsOn(DbResetForce)
    .DependsOn(InstallDotnetToolLiquidReports)
    .DependsOn(RunLocalCleanDirectories)
    .Executes(() =>
    {
      Log.Information("Running E2E tests with Docker Compose");

      int exitCode = 0;
      try
      {
        var args = SkipPublish
          ? "compose -f Test/e2e/docker-compose.yml -f Test/e2e/docker-compose.ci.yml up --no-build --abort-on-container-exit"
          : "compose -f Test/e2e/docker-compose.yml up --build --abort-on-container-exit";
        var envVars = new Dictionary<string, string> { ["DOCKER_BUILDKIT"] = "1", };
        var process = ProcessTasks.StartProcess(
          "docker",
          args,
          workingDirectory: RootDirectory,
          environmentVariables: envVars);
        process.WaitForExit();
        exitCode = process.ExitCode;
      }
      finally
      {
        var downArgs = SkipPublish ?
          "compose -f Test/e2e/docker-compose.yml -f Test/e2e/docker-compose.ci.yml down" :
          "compose -f Test/e2e/docker-compose.yml down";
        var downProcess = ProcessTasks.StartProcess(
          "docker",
          downArgs,
          workingDirectory: RootDirectory);
        downProcess.WaitForExit();
      }

      // Generate LiquidTestReport from TRX files
      var reportFile = ReportsTestE2eArtifactsDirectory / "Report.md";

      try
      {
        Liquid(
          $"--inputs \"File=*.trx;Folder={ReportsTestE2eResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestE2e)} Results\"");

        // Extract summary from Report.md (everything before first "---")
        var reportSummaryFile = ReportsTestE2eArtifactsDirectory / "ReportSummary.md";
        ExtractReportSummary(reportFile, reportSummaryFile);
      }
      catch (Exception ex)
      {
        Log.Warning("Failed to generate LiquidTestReport: {Message}", ex.Message);
      }

      // Explicitly fail the target if Docker Compose failed
      if (exitCode != 0)
      {
        const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/e2e/Artifacts/ReportSummary.md. Then view logs in Logs/Test/e2e/Server.Web/Serilog to diagnose specific failures.";
        Log.Error("E2E tests failed. {DebugInstructions}", debugInstructions);
        throw new Exception($"E2E tests failed with exit code: {exitCode}. {debugInstructions}");
      }
    });

    return _;
  };

  // LiquidTestReports.Cli dotnet global tool isn't available as a built-in Nuke tool under Nuke.Common.Tools, so we resolve it manually
  private Tool Liquid => ToolResolver.GetPathTool("liquid");

  private void RunPostmanCollection(string collectionName)
  {
    Log.Information("Running Postman {CollectionName} tests with Docker Compose{BailSuffix}", collectionName, Bail ? " (with --bail)" : string.Empty);

    var envVars = new Dictionary<string, string>
    {
      ["DOCKER_BUILDKIT"] = "1",
    };

    if (Bail)
    {
      envVars["NEWMAN_BAIL"] = "true";
    }

    int exitCode = 0;
    try
    {
      var args = $"compose -f Test/Postman/docker-compose.yml -f Test/Postman/docker-compose.{collectionName}.yml up --build --abort-on-container-exit";
      var process = ProcessTasks.StartProcess(
            "docker",
            args,
            workingDirectory: RootDirectory,
            environmentVariables: envVars);
      process.WaitForExit();
      exitCode = process.ExitCode;
    }
    finally
    {
      var downArgs = $"compose -f Test/Postman/docker-compose.yml -f Test/Postman/docker-compose.{collectionName}.yml down";
      var downEnvVars = new Dictionary<string, string>
      {
        ["DOCKER_BUILDKIT"] = "1",
      };
      var downProcess = ProcessTasks.StartProcess(
            "docker",
            downArgs,
            workingDirectory: RootDirectory,
            environmentVariables: downEnvVars);
      downProcess.WaitForExit();
    }

    // Generate markdown report and summary from Newman JSON report
    var newmanReportFile = ReportsTestPostmanDirectory / collectionName / "Results" / "newman-report.json";
    var reportFile = ReportsTestPostmanDirectory / collectionName / "Artifacts" / "Report.md";
    var reportSummaryFile = ReportsTestPostmanDirectory / collectionName / "Artifacts" / "ReportSummary.md";

    if (newmanReportFile.FileExists())
    {
      try
      {
        GenerateNewmanReport(newmanReportFile, reportFile, reportSummaryFile, collectionName, Bail);
      }
      catch (Exception ex)
      {
        Log.Warning("Failed to generate Newman report for {CollectionName}: {Message}", collectionName, ex.Message);
      }
    }

    // Explicitly fail the target if Docker Compose failed
    if (exitCode != 0)
    {
      var debugInstructions = $"For a high-level summary of specific failures, see Reports/Test/Postman/{collectionName}/Artifacts/ReportSummary.md. Then view logs in Logs/Test/Postman/{collectionName}/Server.Web/Serilog to diagnose specific failures.";
      Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
      Log.Error("Test failed. {DebugInstructions}", debugInstructions);
      throw new Exception($"Postman {collectionName} tests failed with exit code: {exitCode}. {debugInstructions}");
    }
  }

  /// <summary>
  /// Extracts the summary section from a LiquidTestReport Report.md file.
  /// The summary is everything before the first "---" separator.
  ///
  /// In the CI pipeline we output the full report to the job summary,
  /// and we append the link to the full report to this ReportSummary.md.
  /// That way the PR comment shows the high level pass/fail stats,
  /// and allows developers to click through to the full report if needed.
  ///
  /// This is necessary since having the full report in the comment wont scale
  /// and is likely to hit size limits on comments.
  /// </summary>
  private void ExtractReportSummary(AbsolutePath reportFile, AbsolutePath summaryFile)
  {
    if (!reportFile.FileExists())
    {
      Log.Warning("Report file not found: {ReportFile}", reportFile);
      return;
    }

    var lines = reportFile.ReadAllLines();
    var summaryLines = new List<string>();

    foreach (var line in lines)
    {
      if (line.Trim() == "---")
      {
        break;
      }

      summaryLines.Add(line);
    }

    summaryFile.WriteAllLines(summaryLines);
    Log.Information("Extracted report summary to: {SummaryFile}", summaryFile);
  }

  /// <summary>
  /// Generates both full report and summary markdown files from a Newman JSON report.
  /// The full report includes detailed test results for all executions.
  /// The summary is a condensed version with just high-level stats and failures.
  /// </summary>
  private void GenerateNewmanReport(AbsolutePath newmanReportFile, AbsolutePath reportFile, AbsolutePath summaryFile, string collectionName, bool bail)
  {
    if (!newmanReportFile.FileExists())
    {
      Log.Warning("Newman report file not found: {ReportFile}", newmanReportFile);
      return;
    }

    // Ensure the Artifacts directory exists
    reportFile.Parent.CreateDirectory();
    summaryFile.Parent.CreateDirectory();

    var jsonContent = newmanReportFile.ReadAllText();
    using var jsonDoc = JsonDocument.Parse(jsonContent);
    var root = jsonDoc.RootElement;

    if (!root.TryGetProperty("run", out var run))
    {
      Log.Warning("Invalid Newman report format: missing 'run' property");
      return;
    }

    var stats = run.GetProperty("stats");
    var assertions = stats.GetProperty("assertions");
    var requests = stats.GetProperty("requests");

    var totalTests = assertions.GetProperty("total").GetInt32();
    var failedTests = assertions.GetProperty("failed").GetInt32();
    var passedTests = totalTests - failedTests;

    var totalRequests = requests.GetProperty("total").GetInt32();
    var failedRequests = requests.GetProperty("failed").GetInt32();
    var passedRequests = totalRequests - failedRequests;

    var timings = run.GetProperty("timings");
    var started = timings.GetProperty("started").GetInt64();
    var completed = timings.GetProperty("completed").GetInt64();
    var executionTimeMs = completed - started;
    var executionTimeSec = Math.Round(executionTimeMs / 1000.0, 1);

    var testPassPercentage = totalTests > 0 ? Math.Round(passedTests * 100.0 / totalTests) : 0;
    var statusIcon = failedTests == 0 ? "✅" : "❌";
    var statusText = failedTests == 0 ? "PASSED" : "FAILED";
    var bailText = bail ? " --bail" : string.Empty;

    // Generate the summary (header + stats + failures)
    var summaryLines = new List<string>
    {
      $"## {statusIcon} Postman API Tests ({collectionName}{bailText}) {statusText}",
      string.Empty,
      "**📊 Test Summary**",
      $"- **Tests**: {passedTests}/{totalTests} passed ({testPassPercentage}%)",
      $"- **Requests**: {passedRequests}/{totalRequests} passed",
      $"- **Execution Time**: {executionTimeSec}s",
      string.Empty,
    };

    // Add failure details if any
    if (run.TryGetProperty("failures", out var failures) && failures.GetArrayLength() > 0)
    {
      summaryLines.Add("**🔍 All Failures**");
      foreach (var failure in failures.EnumerateArray())
      {
        var source = failure.TryGetProperty("source", out var src) ? src : default;
        var error = failure.TryGetProperty("error", out var err) ? err : default;

        var sourceName = source.ValueKind != JsonValueKind.Undefined && source.TryGetProperty("name", out var name)
          ? name.GetString()
          : "Unknown1";

        var errorMessage = error.ValueKind != JsonValueKind.Undefined && error.TryGetProperty("message", out var msg)
          ? msg.GetString()
          : (error.ValueKind != JsonValueKind.Undefined && error.TryGetProperty("test", out var test)
            ? test.GetString()
            : "Unknown error");

        // Try to extract correlation ID from error.test (which contains the assertion name) or error.message
        var testName = error.ValueKind != JsonValueKind.Undefined && error.TryGetProperty("test", out var errorTest)
          ? errorTest.GetString()
          : string.Empty;

        var correlationId = !string.IsNullOrEmpty(testName)
          ? ExtractCorrelationId(testName)
          : ExtractCorrelationId(errorMessage ?? string.Empty);

        var cleanMessage = RemoveCorrelationId(errorMessage ?? string.Empty);

        if (!string.IsNullOrEmpty(correlationId))
        {
          summaryLines.Add($"• **{sourceName}** `[CorrelationId: {correlationId}]`: {cleanMessage}");
        }
        else
        {
          Log.Error("Error while processing newman-report.json no correlationId found for failure: {sourceName}: {cleanMessage}", sourceName, cleanMessage);
          throw new Exception($"Error while processing newman-report.json no correlationId found for failure - {sourceName}: {cleanMessage}");

          // summaryLines.Add($"• **{sourceName}**: {cleanMessage}");
        }
      }
    }
    else
    {
      summaryLines.Add("**🎉 All tests passed!**");
    }

    // Write summary file
    summaryFile.WriteAllLines(summaryLines);
    Log.Information("Generated Newman report summary to: {SummaryFile}", summaryFile);

    // Generate full report with detailed execution information
    var reportLines = new List<string>(summaryLines);
    reportLines.Add(string.Empty);
    reportLines.Add("---");
    reportLines.Add(string.Empty);

    // Add detailed execution information
    if (run.TryGetProperty("executions", out var executions) && executions.GetArrayLength() > 0)
    {
      reportLines.Add("## 📋 Detailed Test Results");
      reportLines.Add(string.Empty);

      foreach (var execution in executions.EnumerateArray())
      {
        if (execution.TryGetProperty("item", out var item) && item.TryGetProperty("name", out var itemName))
        {
          var name = itemName.GetString() ?? "Unknown2";
          reportLines.Add($"### {name}");
          reportLines.Add(string.Empty);

          // Add request information
          if (execution.TryGetProperty("request", out var request))
          {
            if (request.TryGetProperty("method", out var method) && request.TryGetProperty("url", out var url))
            {
              var methodStr = method.GetString() ?? "?";
              var urlStr = ConstructUrlFromObject(url);
              reportLines.Add($"**Request**: `{methodStr} {urlStr}`");
              reportLines.Add(string.Empty);
            }
          }

          // Add response information
          if (execution.TryGetProperty("response", out var response))
          {
            if (response.TryGetProperty("code", out var code) && response.TryGetProperty("status", out var status))
            {
              var codeInt = code.GetInt32();
              var statusStr = status.GetString() ?? "?";
              reportLines.Add($"**Response**: {codeInt} {statusStr}");
              reportLines.Add(string.Empty);
            }
          }

          // Add assertions
          if (execution.TryGetProperty("assertions", out var executionAssertions) && executionAssertions.GetArrayLength() > 0)
          {
            reportLines.Add("**Assertions**:");
            foreach (var assertion in executionAssertions.EnumerateArray())
            {
              var assertionName = assertion.TryGetProperty("assertion", out var aName) ? aName.GetString() : "Unknown4";
              var skipped = assertion.TryGetProperty("skipped", out var skip) && skip.GetBoolean();

              // Extract correlation ID from assertion name if present
              var correlationId = ExtractCorrelationId(assertionName ?? string.Empty);
              var cleanAssertionName = RemoveCorrelationId(assertionName ?? "Unknown5");

              if (skipped)
              {
                reportLines.Add($"- ⊘ {cleanAssertionName} (skipped)");
              }
              else if (assertion.TryGetProperty("error", out var assertionError) && assertionError.ValueKind != JsonValueKind.Null && assertionError.ValueKind != JsonValueKind.Undefined)
              {
                var errorMsg = assertionError.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                if (!string.IsNullOrEmpty(correlationId))
                {
                  reportLines.Add($"- ❌ {cleanAssertionName} `[CorrelationId: {correlationId}]`: {errorMsg}");
                }
                else
                {
                  reportLines.Add($"- ❌ {cleanAssertionName}: {errorMsg}");
                }
              }
              else
              {
                reportLines.Add($"- ✅ {cleanAssertionName}");
              }
            }

            reportLines.Add(string.Empty);
          }

          reportLines.Add("---");
          reportLines.Add(string.Empty);
        }
      }
    }

    // Write full report file
    reportFile.WriteAllLines(reportLines);
    Log.Information("Generated full Newman report to: {ReportFile}", reportFile);
  }

  /// <summary>
  /// Extracts correlation ID from a test name in format: [correlation-id] test name
  /// </summary>
  private string ExtractCorrelationId(string testName)
  {
    if (string.IsNullOrEmpty(testName))
    {
      return string.Empty;
    }

    var match = System.Text.RegularExpressions.Regex.Match(testName, @"^\[([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\]");
    return match.Success ? match.Groups[1].Value : string.Empty;
  }

  /// <summary>
  /// Removes correlation ID prefix from a test name
  /// </summary>
  private string RemoveCorrelationId(string testName)
  {
    if (string.IsNullOrEmpty(testName))
    {
      return testName;
    }

    return System.Text.RegularExpressions.Regex.Replace(testName, @"^\[([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})\]\s*", string.Empty);
  }

  /// <summary>
  /// Constructs a URL string from a Newman URL object
  /// </summary>
  private string ConstructUrlFromObject(JsonElement url)
  {
    // If it's already a string, return it
    if (url.ValueKind == JsonValueKind.String)
    {
      return url.GetString() ?? "Unknown";
    }

    // Try to get raw URL first
    if (url.TryGetProperty("raw", out var rawUrl) && rawUrl.ValueKind == JsonValueKind.String)
    {
      return rawUrl.GetString() ?? "Unknown";
    }

    // Construct URL from components
    var protocol = url.TryGetProperty("protocol", out var proto) ? proto.GetString() : "http";
    var host = url.TryGetProperty("host", out var hostArray) && hostArray.ValueKind == JsonValueKind.Array
      ? string.Join(".", hostArray.EnumerateArray().Select(h => h.GetString() ?? string.Empty))
      : "unknown";
    var port = url.TryGetProperty("port", out var portVal) ? portVal.GetString() : string.Empty;
    var path = url.TryGetProperty("path", out var pathArray) && pathArray.ValueKind == JsonValueKind.Array
      ? "/" + string.Join("/", pathArray.EnumerateArray().Select(p => p.GetString() ?? string.Empty))
      : string.Empty;

    var urlStr = $"{protocol}://{host}";
    if (!string.IsNullOrEmpty(port))
    {
      urlStr += $":{port}";
    }

    urlStr += path;

    return urlStr;
  }
}
