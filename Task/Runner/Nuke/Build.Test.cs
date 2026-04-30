using Nuke;
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

  [Parameter("Stop tests on first test failure")]
  internal readonly bool Bail;

  [Parameter("Suppress verbose output for agent context efficiency")]
  internal readonly bool Agent;

  internal Target TestServer => _ => _
      .Description("Run backend tests and generate test and coverage reports")
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
      .DependsOn(RunLocalDependencies)
      .Executes(() =>
      {
        var testFailed = false;
        try
        {
          var sqlPort = 1433 + Constants.Worktree.GetPortOffset(RootDirectory);
          var connectionString = $"Server=localhost,{sqlPort};Database=Conduit;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;MultipleActiveResultSets=true";
          DotNetTest(s => s
                .SetProjectFile(ServerSolution)
                .SetLoggers("trx")
                .SetResultsDirectory(ReportsServerResultsDirectory)
                .SetSettingsFile(RootDirectory / "App" / "Server" / "coverlet.runsettings")
                .SetProcessEnvironmentVariable("ConnectionStrings__DefaultConnection", connectionString)
                .AddProcessAdditionalArguments(
                  "--collect:\"XPlat Code Coverage\"",
                  "--",
                  $"xUnit.StopOnFail={(Bail ? "true" : "false")}"
                ));
        }
        catch (ProcessException)
        {
          testFailed = true;
        }

        var reportFile = ReportsServerArtifactsDirectory / "Tests" / "Report.md";

        // Run coverage report generation in parallel with test report generation
        var coverageTask = System.Threading.Tasks.Task.Run(() =>
            ReportGenerator(s => s
                  .SetReports(ReportsServerResultsDirectory / "**" / "coverage.cobertura.xml")
                  .SetTargetDirectory(ReportsServerArtifactsDirectory / "Coverage")
                  .SetReportTypes(ReportTypes.Html, ReportTypes.Cobertura)));

        Liquid($"--inputs \"File=*.trx;Folder={ReportsServerResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestServer)} Results\"");

        // Extract summary from Report.md (everything before first "---")
        var reportSummaryFile = ReportsServerArtifactsDirectory / "Tests" / "ReportSummary.md";
        ExtractReportSummary(reportFile, reportSummaryFile);

        if (Agent)
        {
          PrintReportSummary(reportSummaryFile);
        }

        coverageTask.Wait();

        if (testFailed)
        {
          var debugInstructions = $"For a details of specific failures, see {reportFile}. Then view logs via `cat {LogsTestServerSerilogDirectory}/*.json | grep 'Test_Name_Goes_Here'`";

          Log.Error("Some test projects failed. {DebugInstructions}", debugInstructions);
          throw new Exception($"Some test projects failed. {debugInstructions}");
        }
      });

  internal Target TestClient => _ => _
      .Description("Run client tests")
      .DependsOn(InstallClient)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
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

          if (Agent)
          {
            PrintReportSummary(reportSummaryFile);
          }
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

  internal Target TestE2e => _ =>
  {
    _.Description("Run E2E Playwright tests using Docker Compose")
    .DependsOn(BuildServerPublish)
    .DependsOn(InstallDotnetToolLiquidReports)
    .DependsOn(PathsCleanDirectories)
    .Executes(() =>
    {
      Log.Information("Running E2E tests with Docker Compose");

      var composeFiles = SkipPublish
          ? "-f Test/e2e/docker-compose.yml -f Test/e2e/docker-compose.ci.yml"
          : "-f Test/e2e/docker-compose.yml";
      var upArgs = SkipPublish
          ? $"compose {composeFiles} up --no-build --abort-on-container-exit"
          : $"compose {composeFiles} up --build --abort-on-container-exit";
      var downArgs = $"compose {composeFiles} down";
      int exitCode = 0;
      try
      {
        var envVars = new Dictionary<string, string>(GetWorktreeEnvVars()) { ["DOCKER_BUILDKIT"] = "1", };
        var process = ProcessTasks.StartProcess(
          "docker",
          upArgs,
          workingDirectory: RootDirectory,
          environmentVariables: envVars,
          logOutput: !Agent);
        process.WaitForExit();
        exitCode = process.ExitCode;

        var apiExitCode = GetServiceExitCode(composeFiles, "api");
        if (apiExitCode > 0 && exitCode == 0)
        {
          Log.Error("API container crashed with exit code {ApiExitCode} but Docker Compose reported success", apiExitCode);
          exitCode = apiExitCode;
        }
      }
      finally
      {
        // Start docker compose down in background while report generation proceeds
        var downEnvVars = GetWorktreeEnvVars();
        var downProcess = ProcessTasks.StartProcess(
          "docker",
          downArgs,
          workingDirectory: RootDirectory,
          environmentVariables: downEnvVars,
          logOutput: !Agent);

        // Generate LiquidTestReport from TRX files in parallel with compose down
        var reportFile = ReportsTestE2eArtifactsDirectory / "Report.md";

        try
        {
          Liquid(
            $"--inputs \"File=*.trx;Folder={ReportsTestE2eResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestE2e)} Results\"");

          // Extract summary from Report.md (everything before first "---")
          var reportSummaryFile = ReportsTestE2eArtifactsDirectory / "ReportSummary.md";
          ExtractReportSummary(reportFile, reportSummaryFile);

          if (Agent)
          {
            PrintReportSummary(reportSummaryFile);
          }
        }
        catch (Exception ex)
        {
          Log.Warning("Failed to generate LiquidTestReport: {Message}", ex.Message);
        }

        downProcess.WaitForExit();
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

  internal Target TestE2eAuth => _ => _
      .Description("Run E2E Playwright tests for Auth tier (LoginPage, RegisterPage, SwaggerPage, SettingsPage, UsersPage)")
      .DependsOn(BuildServerPublish)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
      .Executes(() => RunE2eCollection("Auth"));

  internal Target TestE2eArticles => _ => _
      .Description("Run E2E Playwright tests for Articles tier (EditorPage, ArticlePage, ProfilePage)")
      .DependsOn(BuildServerPublish)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
      .Executes(() => RunE2eCollection("Articles"));

  internal Target TestE2eFeed => _ => _
      .Description("Run E2E Playwright tests for Feed tier (HomePage)")
      .DependsOn(BuildServerPublish)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
      .Executes(() => RunE2eCollection("Feed"));

  internal Target TestE2eMultitenancy => _ => _
      .Description("Run E2E Playwright tests for Multitenancy tier")
      .DependsOn(BuildServerPublish)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
      .Executes(() => RunE2eCollection("Multitenancy"));

  internal Target TestE2eMobile => _ => _
      .Description("Run E2E Playwright tests for Mobile responsive tier (navigation, screenshots at 375x667)")
      .DependsOn(BuildServerPublish)
      .DependsOn(InstallDotnetToolLiquidReports)
      .DependsOn(PathsCleanDirectories)
      .Executes(() => RunE2eCollection("Mobile"));

  private void RunE2eCollection(string collectionName)
  {
    Log.Information("Running E2E {CollectionName} tests with Docker Compose", collectionName);

    var composeFiles = SkipPublish
        ? $"-f Test/e2e/docker-compose.yml -f Test/e2e/docker-compose.{collectionName}.yml -f Test/e2e/docker-compose.ci.yml"
        : $"-f Test/e2e/docker-compose.yml -f Test/e2e/docker-compose.{collectionName}.yml";
    var upArgs = SkipPublish
        ? $"compose {composeFiles} up --no-build --abort-on-container-exit"
        : $"compose {composeFiles} up --build --abort-on-container-exit";
    var downArgs = $"compose {composeFiles} down";
    int exitCode = 0;
    try
    {
      var envVars = new Dictionary<string, string>(GetWorktreeEnvVars()) { ["DOCKER_BUILDKIT"] = "1", };
      var process = ProcessTasks.StartProcess(
        "docker",
        upArgs,
        workingDirectory: RootDirectory,
        environmentVariables: envVars,
        logOutput: !Agent);
      process.WaitForExit();
      exitCode = process.ExitCode;

      var apiExitCode = GetServiceExitCode(composeFiles, "api");
      if (apiExitCode > 0 && exitCode == 0)
      {
        Log.Error("API container crashed with exit code {ApiExitCode} but Docker Compose reported success", apiExitCode);
        exitCode = apiExitCode;
      }
    }
    finally
    {
      var downEnvVars = GetWorktreeEnvVars();
      var downProcess = ProcessTasks.StartProcess(
        "docker",
        downArgs,
        workingDirectory: RootDirectory,
        environmentVariables: downEnvVars,
        logOutput: !Agent);

      var reportFile = ReportsTestE2eArtifactsDirectory / "Report.md";

      try
      {
        Liquid(
          $"--inputs \"File=*.trx;Folder={ReportsTestE2eResultsDirectory}\" --output-file {reportFile} --title \"nuke TestE2e{collectionName} Results\"");

        var reportSummaryFile = ReportsTestE2eArtifactsDirectory / "ReportSummary.md";
        ExtractReportSummary(reportFile, reportSummaryFile);

        if (Agent)
        {
          PrintReportSummary(reportSummaryFile);
        }
      }
      catch (Exception ex)
      {
        Log.Warning("Failed to generate LiquidTestReport: {Message}", ex.Message);
      }

      downProcess.WaitForExit();
    }

    if (exitCode != 0)
    {
      const string debugInstructions = "For a high-level summary of specific failures, see Reports/Test/e2e/Artifacts/ReportSummary.md. Then view logs in Logs/Test/e2e/Server.Web/Serilog to diagnose specific failures.";
      Log.Error("E2E {CollectionName} tests failed. {DebugInstructions}", collectionName, debugInstructions);
      throw new Exception($"E2E {collectionName} tests failed with exit code: {exitCode}. {debugInstructions}");
    }
  }

  internal Target TestEvalsGenerate => _ => _
    .Description("Generate eval expectations from SPEC-REFERENCE.md using Claude")
    .Executes(() =>
    {
      var specFile = RootDirectory / "SPEC-REFERENCE.md";
      if (!specFile.FileExists())
      {
        throw new Exception($"Spec file not found: {specFile}. TestEvalsGenerate requires SPEC-REFERENCE.md at the repo root.");
      }

      ReportsTestEvalsDirectory.CreateDirectory();

      var script = RootDirectory / "scripts" / "evals" / "generate-expectations.sh";
      if (!script.FileExists())
      {
        throw new Exception($"Script not found: {script}");
      }

      Log.Information("Generating eval expectations from {SpecFile}", specFile);

      var process = ProcessTasks.StartProcess(
        "bash",
        $"{script} --spec {specFile} --output {ReportsTestEvalsExpectationsFile}",
        workingDirectory: RootDirectory,
        logOutput: !Agent);
      process.WaitForExit();

      if (process.ExitCode != 0)
      {
        throw new Exception($"generate-expectations.sh failed with exit code {process.ExitCode}");
      }

      Log.Information("Expectations written to {File}", ReportsTestEvalsExpectationsFile);
    });

  internal Target TestEvalsMasterList => _ => _
    .Description("Extract master test coverage list from [TestCoverage] attributes in E2E tests")
    .Executes(() =>
    {
      var testsDir = RootDirectory / "Test" / "e2e" / "E2eTests" / "Tests";
      if (!testsDir.DirectoryExists())
      {
        throw new Exception($"Tests directory not found: {testsDir}");
      }

      ReportsTestEvalsDirectory.CreateDirectory();
      var masterListFile = ReportsTestEvalsDirectory / "master-list.json";

      Log.Information("Extracting [TestCoverage] attributes from {TestsDir}", testsDir);

      var tests = new List<Dictionary<string, object>>();
      var testFiles = testsDir.GlobFiles("**/*.cs");

      var attrPattern = new System.Text.RegularExpressions.Regex(
        @"\[TestCoverage\(\s*" +
        @"Id\s*=\s*""(?<id>[^""]+)""\s*,\s*" +
        @"FeatureArea\s*=\s*""(?<area>[^""]+)""\s*,\s*" +
        @"Behavior\s*=\s*""(?<behavior>[^""]+)""" +
        @"(?:\s*,\s*Verifies\s*=\s*\[(?<verifies>[^\]]*)\])?" +
        @"\s*\)\]",
        System.Text.RegularExpressions.RegexOptions.Singleline);

      var methodPattern = new System.Text.RegularExpressions.Regex(
        @"public\s+async\s+Task\s+(?<name>\w+)\s*\(");

      foreach (var file in testFiles)
      {
        var content = file.ReadAllText();
        var relativePath = RootDirectory.GetRelativePathTo(file);

        // Extract class namespace/name from file path
        var pathParts = relativePath.ToString().Replace("\\", "/").Split('/');
        var pageName = pathParts.Length >= 2 ? pathParts[^2] : "Unknown";
        var fileName = file.NameWithoutExtension;
        var category = fileName.ToLowerInvariant() switch
        {
          "happypath" => "happy_path",
          "validation" => "validation",
          "permissions" => "permissions",
          "screenshots" => "screenshots",
          _ => fileName.ToLowerInvariant(),
        };

        var attrMatches = attrPattern.Matches(content);
        var methodMatches = methodPattern.Matches(content);

        // Pair each [TestCoverage] with its following method name
        foreach (System.Text.RegularExpressions.Match attrMatch in attrMatches)
        {
          var id = attrMatch.Groups["id"].Value;
          var area = attrMatch.Groups["area"].Value;
          var behavior = attrMatch.Groups["behavior"].Value;
          var verifiesRaw = attrMatch.Groups["verifies"].Value;

          // Parse Verifies array: ["item1", "item2"]
          var verifies = new List<string>();
          if (!string.IsNullOrWhiteSpace(verifiesRaw))
          {
            var verifyPattern = new System.Text.RegularExpressions.Regex(@"""([^""]+)""");
            foreach (System.Text.RegularExpressions.Match v in verifyPattern.Matches(verifiesRaw))
            {
              verifies.Add(v.Groups[1].Value);
            }
          }

          // Find the method name that follows this attribute
          var methodName = "Unknown";
          foreach (System.Text.RegularExpressions.Match methodMatch in methodMatches)
          {
            if (methodMatch.Index > attrMatch.Index)
            {
              methodName = methodMatch.Groups["name"].Value;
              break;
            }
          }

          tests.Add(new Dictionary<string, object>
          {
            ["id"] = id,
            ["test_method"] = methodName,
            ["file"] = relativePath.ToString(),
            ["feature_area"] = area,
            ["category"] = category,
            ["behavior"] = behavior,
            ["verifies"] = verifies,
          });
        }
      }

      // Build JSON output
      var jsonOptions = new System.Text.Json.JsonSerializerOptions
      {
        WriteIndented = true,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
      };

      var masterList = new Dictionary<string, object>
      {
        ["generated_at"] = DateTime.UtcNow.ToString("o"),
        ["source"] = testsDir.ToString(),
        ["test_count"] = tests.Count,
        ["by_feature"] = tests.GroupBy(t => (string)t["feature_area"])
          .ToDictionary(g => g.Key, g => g.Count()),
        ["by_category"] = tests.GroupBy(t => (string)t["category"])
          .ToDictionary(g => g.Key, g => g.Count()),
        ["tests"] = tests,
      };

      var json = System.Text.Json.JsonSerializer.Serialize(masterList, jsonOptions);
      masterListFile.WriteAllText(json);

      Log.Information("Extracted {Count} test coverage entries to {File}", tests.Count, masterListFile);

      if (tests.Count == 0)
      {
        throw new Exception(
          "No [TestCoverage] attributes found. Ensure all [Fact] test methods have " +
          "[TestCoverage] attributes (enforced by E2E008 analyzer).");
      }
    });

  internal Target TestEvalsCoverage => _ => _
    .Description("Assert generated expectations cover all tests in the master list")
    .Executes(() =>
    {
      var masterListFile = ReportsTestEvalsDirectory / "master-list.json";
      if (!masterListFile.FileExists())
      {
        throw new Exception(
          $"Master list not found: {masterListFile}. " +
          "Run ./build.sh TestEvalsMasterList first.");
      }

      if (!ReportsTestEvalsExpectationsFile.FileExists())
      {
        throw new Exception(
          $"Expectations file not found: {ReportsTestEvalsExpectationsFile}. " +
          "Run ./build.sh TestEvalsGenerate first.");
      }

      var script = RootDirectory / "scripts" / "evals" / "eval-coverage.sh";
      if (!script.FileExists())
      {
        throw new Exception($"Script not found: {script}");
      }

      var coverageReportFile = ReportsTestEvalsDirectory / "coverage-report.json";

      Log.Information("Comparing expectations against master list");

      var process = ProcessTasks.StartProcess(
        "bash",
        $"{script} {masterListFile} {ReportsTestEvalsExpectationsFile} --output {coverageReportFile}",
        workingDirectory: RootDirectory,
        logOutput: true);
      process.WaitForExit();

      if (coverageReportFile.FileExists())
      {
        var reportContent = coverageReportFile.ReadAllText();
        var doc = System.Text.Json.JsonDocument.Parse(reportContent);
        var score = doc.RootElement.GetProperty("score").GetProperty("coverage").GetDouble();
        var missing = doc.RootElement.GetProperty("summary").GetProperty("missing").GetInt32();

        Log.Information("Coverage score: {Score}/100", score);

        if (missing > 0)
        {
          Log.Error("{Missing} test cases from the master list are not covered by generated expectations", missing);
          Log.Error("See {File} for details", coverageReportFile);
        }
      }

      if (process.ExitCode != 0)
      {
        throw new Exception(
          $"Coverage check failed — generated expectations do not fully cover the master test list. " +
          $"See {coverageReportFile} for gaps.");
      }

      Log.Information("All master list tests are covered by generated expectations");
    });

  internal Target TestEvals => _ => _
    .Description("Run E2E tests with tracing enabled, then grade traces against spec expectations")
    .DependsOn(BuildServerPublish)
    .DependsOn(InstallDotnetToolLiquidReports)
    .DependsOn(PathsCleanDirectories)
    .Executes(() =>
    {
      // Ensure expectations exist
      if (!ReportsTestEvalsExpectationsFile.FileExists())
      {
        throw new Exception(
          $"Expectations file not found: {ReportsTestEvalsExpectationsFile}. " +
          "Run ./build.sh TestEvalsGenerate first to generate expectations from the spec.");
      }

      // Step 1: Run E2E tests with always-on tracing
      Log.Information("Running E2E tests with PLAYWRIGHT_ALWAYS_TRACE=true");

      var composeFiles = SkipPublish
        ? "-f Test/e2e/docker-compose.yml -f Test/e2e/docker-compose.ci.yml"
        : "-f Test/e2e/docker-compose.yml";
      var upArgs = SkipPublish
        ? $"compose {composeFiles} up --no-build --abort-on-container-exit"
        : $"compose {composeFiles} up --build --abort-on-container-exit";
      var downArgs = $"compose {composeFiles} down";

      int exitCode = 0;
      try
      {
        var envVars = new Dictionary<string, string>(GetWorktreeEnvVars())
        {
          ["DOCKER_BUILDKIT"] = "1",
          ["PLAYWRIGHT_ALWAYS_TRACE"] = "true",
        };

        var process = ProcessTasks.StartProcess(
          "docker",
          upArgs,
          workingDirectory: RootDirectory,
          environmentVariables: envVars,
          logOutput: !Agent);
        process.WaitForExit();
        exitCode = process.ExitCode;

        var apiExitCode = GetServiceExitCode(composeFiles, "api");
        if (apiExitCode > 0 && exitCode == 0)
        {
          Log.Error("API container crashed with exit code {ApiExitCode} but Docker Compose reported success", apiExitCode);
          exitCode = apiExitCode;
        }
      }
      finally
      {
        var downEnvVars = GetWorktreeEnvVars();
        var downProcess = ProcessTasks.StartProcess(
          "docker",
          downArgs,
          workingDirectory: RootDirectory,
          environmentVariables: downEnvVars,
          logOutput: !Agent);

        // Generate test report while compose tears down
        var reportFile = ReportsTestE2eArtifactsDirectory / "Report.md";
        try
        {
          Liquid(
            $"--inputs \"File=*.trx;Folder={ReportsTestE2eResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestEvals)} E2E Results\"");

          var reportSummaryFile = ReportsTestE2eArtifactsDirectory / "ReportSummary.md";
          ExtractReportSummary(reportFile, reportSummaryFile);

          if (Agent)
          {
            PrintReportSummary(reportSummaryFile);
          }
        }
        catch (Exception ex)
        {
          Log.Warning("Failed to generate LiquidTestReport: {Message}", ex.Message);
        }

        downProcess.WaitForExit();
      }

      if (exitCode != 0)
      {
        Log.Warning("E2E tests exited with code {ExitCode} — continuing to grade available traces", exitCode);
      }

      // Step 2: Grade traces against expectations
      var tracesDir = ReportsTestE2eArtifactsDirectory;
      var traceFiles = tracesDir.GlobFiles("*_trace_*.zip");

      if (traceFiles.Count == 0)
      {
        Log.Warning("No trace files found in {Dir}. Ensure PLAYWRIGHT_ALWAYS_TRACE is being passed through docker-compose.", tracesDir);
        throw new Exception("No trace files produced — eval grading cannot proceed.");
      }

      Log.Information("Found {Count} trace files to grade", traceFiles.Count);

      var evalScript = RootDirectory / "scripts" / "evals" / "eval-traces.sh";
      if (!evalScript.FileExists())
      {
        throw new Exception($"Script not found: {evalScript}");
      }

      var evalProcess = ProcessTasks.StartProcess(
        "bash",
        $"{evalScript} {tracesDir} {ReportsTestEvalsExpectationsFile} --output {ReportsTestEvalsResultsDirectory}",
        workingDirectory: RootDirectory,
        logOutput: true);
      evalProcess.WaitForExit();

      // Read and display eval report
      var evalReportFile = ReportsTestEvalsResultsDirectory / "eval-report.json";
      if (evalReportFile.FileExists())
      {
        var reportContent = evalReportFile.ReadAllText();
        var score = System.Text.Json.JsonDocument.Parse(reportContent)
          .RootElement.GetProperty("score").GetProperty("composite").GetDouble();

        Log.Information("Eval score: {Score}/100", score);
        Log.Information("Full eval report: {File}", evalReportFile);
      }

      if (evalProcess.ExitCode != 0)
      {
        throw new Exception($"eval-traces.sh failed with exit code {evalProcess.ExitCode}");
      }
    });

  private int GetServiceExitCode(string composeFiles, string serviceName)
  {
    try
    {
      var psArgs = $"compose {composeFiles} ps -a -q {serviceName}";
      var psProcess = ProcessTasks.StartProcess(
          "docker",
          psArgs,
          workingDirectory: RootDirectory,
          logOutput: !Agent);
      psProcess.WaitForExit();

      var containerId = psProcess.Output
          .Where(o => o.Type == OutputType.Std)
          .Select(o => o.Text.Trim())
          .FirstOrDefault(s => !string.IsNullOrEmpty(s));

      if (string.IsNullOrEmpty(containerId))
      {
        Log.Warning("Could not find container ID for service {ServiceName}", serviceName);
        return -1;
      }

      var inspectArgs = $"inspect --format={{{{.State.ExitCode}}}} {containerId}";
      var inspectProcess = ProcessTasks.StartProcess(
          "docker",
          inspectArgs,
          workingDirectory: RootDirectory,
          logOutput: !Agent);
      inspectProcess.WaitForExit();

      var exitCodeStr = inspectProcess.Output
          .Where(o => o.Type == OutputType.Std)
          .Select(o => o.Text.Trim())
          .FirstOrDefault(s => !string.IsNullOrEmpty(s));

      if (int.TryParse(exitCodeStr, out var exitCode))
      {
        return exitCode;
      }

      Log.Warning("Could not parse exit code for service {ServiceName}: {ExitCodeStr}", serviceName, exitCodeStr);
      return -1;
    }
    catch (Exception ex)
    {
      Log.Warning("Failed to check exit code for service {ServiceName}: {Message}", serviceName, ex.Message);
      return -1;
    }
  }

  // LiquidTestReports.Cli dotnet global tool isn't available as a built-in Nuke tool under Nuke.Common.Tools, so we resolve it manually
  private Tool Liquid => ToolResolver.GetPathTool("liquid");

  private void PrintReportSummary(AbsolutePath summaryFile)
  {
    if (!summaryFile.FileExists())
    {
      return;
    }

    foreach (var line in summaryFile.ReadAllLines())
    {
      Log.Information(line);
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
}
