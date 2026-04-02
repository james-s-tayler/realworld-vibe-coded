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
