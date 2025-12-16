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
        Log.Information("Running Postman ArticlesEmpty tests with Docker Compose");

        var envVars = new Dictionary<string, string>
        {
          ["DOCKER_BUILDKIT"] = "1",
        };

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/Postman/docker-compose.ArticlesEmpty.yml up --build --abort-on-container-exit";
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
          var downArgs = "compose -f Test/Postman/docker-compose.ArticlesEmpty.yml down";
          var downProcess = ProcessTasks.StartProcess(
                "docker",
                downArgs,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          downProcess.WaitForExit();
        }

        // Generate markdown report summary from Newman JSON report
        var newmanReportFile = ReportsTestPostmanDirectory / "ArticlesEmpty" / "newman-report.json";
        var reportSummaryFile = ReportsTestPostmanDirectory / "ArticlesEmpty" / "Artifacts" / "ReportSummary.md";

        if (newmanReportFile.FileExists())
        {
          try
          {
            GenerateNewmanReportSummary(newmanReportFile, reportSummaryFile);
          }
          catch (Exception ex)
          {
            Log.Warning("Failed to generate Newman report summary: {Message}", ex.Message);
          }
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/Postman/ArticlesEmpty/Artifacts/ReportSummary.md. Then view logs in Logs/Server.Web/Serilog to diagnose specific failures.";
          Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
          Log.Error("Test failed. {DebugInstructions}", debugInstructions);
          throw new Exception($"Postman ArticlesEmpty tests failed with exit code: {exitCode}. {debugInstructions}");
        }
      });

  internal Target TestServerPostmanAuth => _ => _
      .Description("Run postman tests for Auth collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        Log.Information("Running Postman Auth tests with Docker Compose");

        var envVars = new Dictionary<string, string>
        {
          ["DOCKER_BUILDKIT"] = "1",
        };

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/Postman/docker-compose.Auth.yml up --build --abort-on-container-exit";
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
          var downArgs = "compose -f Test/Postman/docker-compose.Auth.yml down";
          var downProcess = ProcessTasks.StartProcess(
                "docker",
                downArgs,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          downProcess.WaitForExit();
        }

        // Generate markdown report summary from Newman JSON report
        var newmanReportFile = ReportsTestPostmanDirectory / "Auth" / "newman-report.json";
        var reportSummaryFile = ReportsTestPostmanDirectory / "Auth" / "Artifacts" / "ReportSummary.md";

        if (newmanReportFile.FileExists())
        {
          try
          {
            GenerateNewmanReportSummary(newmanReportFile, reportSummaryFile);
          }
          catch (Exception ex)
          {
            Log.Warning("Failed to generate Newman report summary: {Message}", ex.Message);
          }
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/Postman/Auth/Artifacts/ReportSummary.md. Then view logs in Logs/Server.Web/Serilog to diagnose specific failures.";
          Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
          Log.Error("Test failed. {DebugInstructions}", debugInstructions);
          throw new Exception($"Postman Auth tests failed with exit code: {exitCode}. {debugInstructions}");
        }
      });

  internal Target TestServerPostmanProfiles => _ => _
      .Description("Run postman tests for Profiles collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        Log.Information("Running Postman Profiles tests with Docker Compose");

        var envVars = new Dictionary<string, string>
        {
          ["DOCKER_BUILDKIT"] = "1",
        };

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/Postman/docker-compose.Profiles.yml up --build --abort-on-container-exit";
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
          var downArgs = "compose -f Test/Postman/docker-compose.Profiles.yml down";
          var downProcess = ProcessTasks.StartProcess(
                "docker",
                downArgs,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          downProcess.WaitForExit();
        }

        // Generate markdown report summary from Newman JSON report
        var newmanReportFile = ReportsTestPostmanDirectory / "Profiles" / "newman-report.json";
        var reportSummaryFile = ReportsTestPostmanDirectory / "Profiles" / "Artifacts" / "ReportSummary.md";

        if (newmanReportFile.FileExists())
        {
          try
          {
            GenerateNewmanReportSummary(newmanReportFile, reportSummaryFile);
          }
          catch (Exception ex)
          {
            Log.Warning("Failed to generate Newman report summary: {Message}", ex.Message);
          }
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/Postman/Profiles/Artifacts/ReportSummary.md. Then view logs in Logs/Server.Web/Serilog to diagnose specific failures.";
          Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
          Log.Error("Test failed. {DebugInstructions}", debugInstructions);
          throw new Exception($"Postman Profiles tests failed with exit code: {exitCode}. {debugInstructions}");
        }
      });

  internal Target TestServerPostmanFeedAndArticles => _ => _
      .Description("Run postman tests for FeedAndArticles collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        Log.Information("Running Postman FeedAndArticles tests with Docker Compose");

        var envVars = new Dictionary<string, string>
        {
          ["DOCKER_BUILDKIT"] = "1",
        };

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/Postman/docker-compose.FeedAndArticles.yml up --build --abort-on-container-exit";
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
          var downArgs = "compose -f Test/Postman/docker-compose.FeedAndArticles.yml down";
          var downProcess = ProcessTasks.StartProcess(
                "docker",
                downArgs,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          downProcess.WaitForExit();
        }

        // Generate markdown report summary from Newman JSON report
        var newmanReportFile = ReportsTestPostmanDirectory / "FeedAndArticles" / "newman-report.json";
        var reportSummaryFile = ReportsTestPostmanDirectory / "FeedAndArticles" / "Artifacts" / "ReportSummary.md";

        if (newmanReportFile.FileExists())
        {
          try
          {
            GenerateNewmanReportSummary(newmanReportFile, reportSummaryFile);
          }
          catch (Exception ex)
          {
            Log.Warning("Failed to generate Newman report summary: {Message}", ex.Message);
          }
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/Postman/FeedAndArticles/Artifacts/ReportSummary.md. Then view logs in Logs/Server.Web/Serilog to diagnose specific failures.";
          Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
          Log.Error("Test failed. {DebugInstructions}", debugInstructions);
          throw new Exception($"Postman FeedAndArticles tests failed with exit code: {exitCode}. {debugInstructions}");
        }
      });

  internal Target TestServerPostmanArticle => _ => _
      .Description("Run postman tests for Article collection using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(RunLocalCleanDirectories)
      .Executes(() =>
      {
        Log.Information("Running Postman Article tests with Docker Compose");

        var envVars = new Dictionary<string, string>
        {
          ["DOCKER_BUILDKIT"] = "1",
        };

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/Postman/docker-compose.Article.yml up --build --abort-on-container-exit";
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
          var downArgs = "compose -f Test/Postman/docker-compose.Article.yml down";
          var downProcess = ProcessTasks.StartProcess(
                "docker",
                downArgs,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          downProcess.WaitForExit();
        }

        // Generate markdown report summary from Newman JSON report
        var newmanReportFile = ReportsTestPostmanDirectory / "Article" / "newman-report.json";
        var reportSummaryFile = ReportsTestPostmanDirectory / "Article" / "Artifacts" / "ReportSummary.md";

        if (newmanReportFile.FileExists())
        {
          try
          {
            GenerateNewmanReportSummary(newmanReportFile, reportSummaryFile);
          }
          catch (Exception ex)
          {
            Log.Warning("Failed to generate Newman report summary: {Message}", ex.Message);
          }
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/Postman/Article/Artifacts/ReportSummary.md. Then view logs in Logs/Server.Web/Serilog to diagnose specific failures.";
          Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
          Log.Error("Test failed. {DebugInstructions}", debugInstructions);
          throw new Exception($"Postman Article tests failed with exit code: {exitCode}. {debugInstructions}");
        }
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
  /// Generates a markdown report summary from a Newman JSON report.
  /// This provides a human-readable summary of test results including
  /// test statistics and failure details.
  /// </summary>
  private void GenerateNewmanReportSummary(AbsolutePath newmanReportFile, AbsolutePath summaryFile)
  {
    if (!newmanReportFile.FileExists())
    {
      Log.Warning("Newman report file not found: {ReportFile}", newmanReportFile);
      return;
    }

    // Ensure the Artifacts directory exists
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

    var summaryLines = new List<string>
    {
      $"## {statusIcon} Postman API Tests {statusText}",
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
          : "Unknown";

        var errorMessage = error.ValueKind != JsonValueKind.Undefined && error.TryGetProperty("message", out var msg)
          ? msg.GetString()
          : (error.ValueKind != JsonValueKind.Undefined && error.TryGetProperty("test", out var test)
            ? test.GetString()
            : "Unknown error");

        summaryLines.Add($"• **{sourceName}**: {errorMessage}");
      }
    }
    else
    {
      summaryLines.Add("**🎉 All tests passed!**");
    }

    summaryFile.WriteAllLines(summaryLines);
    Log.Information("Generated Newman report summary to: {SummaryFile}", summaryFile);
  }
}
