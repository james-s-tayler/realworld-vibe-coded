using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Nuke.Common.Tools.ReportGenerator;
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
        if (Directory.Exists(ReportsServerDirectory))
        {
          Directory.Delete(ReportsServerDirectory, true);
        }

        Directory.CreateDirectory(ReportsServerDirectory);

        // Get all test projects in the solution
        var testsDirectory = RootDirectory / "App" / "Server" / "tests";
        var testProjects = Directory.GetDirectories(testsDirectory)
              .Select(dir => (AbsolutePath)dir / $"{Path.GetFileName(dir)}.csproj")
              .Where(project => File.Exists(project))
              .ToArray();

        var failures = new List<string>();

        // Run tests for each project with unique result files
        foreach (var testProject in testProjects)
        {
          var projectName = Path.GetFileNameWithoutExtension(testProject);
          var logFileName = $"{projectName}-results.trx";

          Console.WriteLine($"Running tests for {projectName}...");

          try
          {
            DotNetTest(s => s
                  .SetProjectFile(testProject)
                  .SetLoggers($"trx;LogFileName={logFileName}")
                  .SetResultsDirectory(ReportsServerResultsDirectory)
                  .AddProcessAdditionalArguments("--collect:\"XPlat Code Coverage\""));
          }
          catch (ProcessException)
          {
            failures.Add(testProject.Name);
          }
        }

        var reportFile = ReportsServerArtifactsDirectory / "Tests" / "Report.md";

        Liquid($"--inputs \"File=*.trx;Folder={ReportsServerResultsDirectory}\" --output-file {reportFile} --title \"nuke {nameof(TestServer)} Results\"");

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
        if (Directory.Exists(ReportsClientDirectory))
        {
          Directory.Delete(ReportsClientDirectory, true);
        }

        Directory.CreateDirectory(ReportsClientResultsDirectory);
        Directory.CreateDirectory(ReportsClientArtifactsDirectory);

        Console.WriteLine($"Running client tests in {ClientDirectory}");

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
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Warning: Failed to generate LiquidTestReport: {ex.Message}");
        }

        if (testsFailed)
        {
          throw new Exception("Client tests failed");
        }
      });

  Target TestServerPostman => _ => _
      .Description("Run postman tests using Docker Compose. Optionally specify a FOLDER parameter to run a specific Postman collection folder. E.g. FOLDER=Auth nuke TestServerPostman")
      .DependsOn(DbResetForce)
      .Executes(() =>
      {
        if (Directory.Exists(ReportsTestPostmanDirectory))
          Directory.Delete(ReportsTestPostmanDirectory, true);
        Directory.CreateDirectory(ReportsTestPostmanDirectory);

        Console.WriteLine("Running Postman tests with Docker Compose...");

        var envVars = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(Folder))
        {
          envVars["FOLDER"] = Folder;
          Console.WriteLine($"Setting FOLDER environment variable to: {Folder}");
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
          Console.WriteLine($"Docker Compose exited with code: {exitCode}");
          throw new Exception($"Postman tests failed with exit code: {exitCode}");
        }
      });

  Target TestE2e => _ => _
      .Description("Run E2E Playwright tests using Docker Compose")
      .DependsOn(DbResetForce)
      .DependsOn(InstallDotnetToolLiquidReports)
      .Executes(() =>
      {
        if (Directory.Exists(ReportsTestE2eDirectory))
        {
          Directory.Delete(ReportsTestE2eDirectory, true);
        }
        Directory.CreateDirectory(ReportsTestE2eResultsDirectory);
        Directory.CreateDirectory(ReportsTestE2eArtifactsDirectory);

        Console.WriteLine("Running E2E tests with Docker Compose...");

        int exitCode = 0;
        try
        {
          var args = "compose -f Test/e2e/docker-compose.yml up --build --abort-on-container-exit";
          var process = ProcessTasks.StartProcess("docker", args,
                workingDirectory: RootDirectory);
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
        }
        catch (Exception ex)
        {
          Console.WriteLine($"Warning: Failed to generate LiquidTestReport: {ex.Message}");
        }

        // Explicitly fail the target if Docker Compose failed
        if (exitCode != 0)
        {
          Console.WriteLine($"E2E tests failed with exit code: {exitCode}");
          throw new Exception($"E2E tests failed with exit code: {exitCode}");
        }
      });
}
