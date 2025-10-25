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
  [Parameter("Postman folder to test")]
  readonly string? Folder;

  // LiquidTestReports.Cli dotnet global tool isn't available as a built-in Nuke tool under Nuke.Common.Tools, so we resolve it manually
  private Tool Liquid => ToolResolver.GetPathTool("liquid");

  Target TestServer => _ => _
      .Description("Run backend tests and generate test and coverage reports")
      .DependsOn(InstallDotnetToolLiquidReports)
      .Executes(() =>
      {
        ReportsServerDirectory.CreateOrCleanDirectory();

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

  Target TestClient => _ => _
      .Description("Run client tests")
      .DependsOn(InstallClient)
      .DependsOn(InstallDotnetToolLiquidReports)
      .Executes(() =>
      {
        ReportsClientDirectory.CreateOrCleanDirectory();
        // explicitly create these so that docker doesn't create them with root permissions making them hard to delete later
        ReportsClientResultsDirectory.CreateOrCleanDirectory();
        ReportsClientArtifactsDirectory.CreateOrCleanDirectory();

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

  Target TestServerPostman => _ => _
      .Description("Run postman tests using Docker Compose. Optionally specify a FOLDER parameter to run a specific Postman collection folder. E.g. FOLDER=Auth nuke TestServerPostman")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .Executes(() =>
      {
        ReportsTestPostmanDirectory.CreateOrCleanDirectory();

        Log.Information("Running Postman tests with Docker Compose");

        var envVars = new Dictionary<string, string>
        {
          ["DOCKER_BUILDKIT"] = "1"
        };
        if (!string.IsNullOrEmpty(Folder))
        {
          envVars["FOLDER"] = Folder;
          Log.Information("Setting FOLDER environment variable to: {Folder}", Folder);
        }

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/Postman/docker-compose.yml up --build --abort-on-container-exit";
          var process = ProcessTasks.StartProcess("docker", args,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          process.WaitForExit();
          exitCode = process.ExitCode;
        }
        finally
        {
          var downArgs = "compose -f Test/Postman/docker-compose.yml down";
          var downProcess = ProcessTasks.StartProcess("docker", downArgs,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          downProcess.WaitForExit();
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          Log.Error("Docker Compose exited with code: {ExitCode}", exitCode);
          throw new Exception($"Postman tests failed with exit code: {exitCode}");
        }
      });

  Target TestE2e => _ => _
      .Description("Run E2E Playwright tests using Docker Compose")
      .DependsOn(BuildServerPublish)
      .DependsOn(DbResetForce)
      .DependsOn(InstallDotnetToolLiquidReports)
      .Executes(() =>
      {
        ReportsTestE2eDirectory.CreateOrCleanDirectory();
        // explicitly create these so that docker doesn't create them with root permissions making them hard to delete later
        ReportsTestE2eResultsDirectory.CreateOrCleanDirectory();
        ReportsTestE2eArtifactsDirectory.CreateOrCleanDirectory();

        Log.Information("Running E2E tests with Docker Compose");

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/e2e/docker-compose.yml up --build --abort-on-container-exit";
          var envVars = new Dictionary<string, string>
          {
            ["DOCKER_BUILDKIT"] = "1"
          };
          var process = ProcessTasks.StartProcess("docker", args,
                workingDirectory: RootDirectory,
                environmentVariables: envVars);
          process.WaitForExit();
          exitCode = process.ExitCode;
        }
        finally
        {
          var downArgs = "compose -f Test/e2e/docker-compose.yml down";
          var downProcess = ProcessTasks.StartProcess("docker", downArgs,
                workingDirectory: RootDirectory);
          downProcess.WaitForExit();
        }

        // Generate LiquidTestReport from TRX files
        var reportFile = ReportsTestE2eArtifactsDirectory / "Report.md";

        try
        {
          Liquid($"--inputs \"File=*.trx;Folder={ReportsTestE2eResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestE2e)} Results\"");

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
          Log.Error("E2E tests failed with exit code: {ExitCode}", exitCode);
          throw new Exception($"E2E tests failed with exit code: {exitCode}");
        }
      });

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
