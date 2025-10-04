using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public class Build : NukeBuild
{
    // LiquidTestReports.Cli dotnet global tool isn't available as a built-in Nuke tool under Nuke.Common.Tools, so we resolve it manually
    private Tool Liquid => ToolResolver.GetPathTool("liquid");

    public static int Main() => Execute<Build>();

    [Parameter("Postman folder to test")]
    readonly string? Folder;

    [Parameter("Force operation without confirmation")]
    readonly bool Force = false;

    // Paths  
    AbsolutePath TaskRunnerDirectory => RootDirectory / "Task" / "Runner";
    AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";
    AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";
    AbsolutePath ClientDirectory => RootDirectory / "App" / "Client";
    AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";
    AbsolutePath ReportsDirectory => RootDirectory / "reports";
    AbsolutePath DatabaseFile => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "database.sqlite";

    Target LintServerVerify => _ => _
        .Description("Verify backend formatting & analyzers (no changes). Fails if issues found")
        .Executes(() =>
        {
            Console.WriteLine($"Running dotnet format (verify only) on {ServerSolution}");
            DotNetFormat(s => s
                .SetProject(ServerSolution)
                .SetVerifyNoChanges(true));
        });

    Target LintServerFix => _ => _
        .Description("Fix backend formatting & analyzer issues automatically")
        .Executes(() =>
        {
            Console.WriteLine($"Running dotnet format (fix mode) on {ServerSolution}");
            DotNetFormat(s => s
                .SetProject(ServerSolution));
        });

    Target LintClientVerify => _ => _
        .Description("Verify client code formatting and style")
        .DependsOn(InstallClient)
        .Executes(() =>
        {
            Console.WriteLine($"Running ESLint on {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("lint"));
        });

    Target LintClientFix => _ => _
        .Description("Fix client code formatting and style issues automatically")
        .DependsOn(InstallClient)
        .Executes(() =>
        {
            Console.WriteLine($"Running ESLint fix on {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("lint:fix"));
        });

    Target LintNukeVerify => _ => _
        .Description("Verify Nuke build targets for documentation and naming conventions")
        .Executes(() =>
        {
            var nukeSolution = TaskRunnerDirectory / "Nuke.sln";

            // Run dotnet format on the Nuke solution (only check whitespace and style, not warnings)
            Console.WriteLine($"Running dotnet format (verify only) on {nukeSolution}");
            DotNetFormat(s => s
                .SetProject(nukeSolution)
                .SetSeverity("error")
                .SetVerifyNoChanges(true));

            // Run the ArchUnit tests
            var testProject = TaskRunnerDirectory / "Nuke.Tests" / "Nuke.Tests.csproj";
            DotNetTest(s => s
                .SetProjectFile(testProject));
        });

    Target LintNukeFix => _ => _
        .Description("Fix Nuke build formatting and style issues automatically")
        .Executes(() =>
        {
            var nukeSolution = TaskRunnerDirectory / "Nuke.sln";

            // Run dotnet format on the Nuke solution to fix formatting issues
            Console.WriteLine($"Running dotnet format (fix mode) on {nukeSolution}");
            DotNetFormat(s => s
                .SetProject(nukeSolution)
                .SetSeverity("error"));
        });

    Target BuildServer => _ => _
        .Description("dotnet build (backend)")
        .Executes(() =>
        {
            DotNetBuild(s => s.SetProjectFile(ServerSolution));
        });

    Target BuildClient => _ => _
        .Description("Build client (frontend)")
        .DependsOn(InstallClient)
        .Executes(() =>
        {
            Console.WriteLine($"Building client in {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("build"));
        });
    
    Target TestServer => _ => _
        .Description("Run backend tests")
        .DependsOn(InstallDotnetToolLiquidReports)
        .Executes(() =>
        {
            if (Directory.Exists(TestResultsDirectory))
            {
                Directory.Delete(TestResultsDirectory, true);
            }

            Directory.CreateDirectory(TestResultsDirectory);

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
                        .SetResultsDirectory(TestResultsDirectory));
                }
                catch (ProcessException)
                {
                    failures.Add(testProject.Name);
                }
            }
            
            var reportFile = TestResultsDirectory / "report.md";

            Liquid($"--inputs \"File=*.trx;Folder={TestResultsDirectory}\" --output-file {reportFile}");
            
            if (failures.Any())
            {
                var failedProjects = string.Join(", ", failures);
                throw new Exception($"Some test projects failed: {failedProjects}");
            }
        });

    Target TestClient => _ => _
        .Description("Run client tests")
        .DependsOn(InstallClient)
        .Executes(() =>
        {
            Console.WriteLine($"Running client tests in {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("test:run"));
        });

    Target TestServerPostman => _ => _
        .Description("Run postman tests using Docker Compose. Optionally specify a FOLDER parameter to run a specific Postman collection folder. E.g. FOLDER=Auth nuke TestServerPostman")
        .DependsOn(DbResetForce)
        .Executes(() =>
        {
            if (Directory.Exists(ReportsDirectory))
                Directory.Delete(ReportsDirectory, true);
            Directory.CreateDirectory(ReportsDirectory);

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
        .Executes(() =>
        {
            if (Directory.Exists(ReportsDirectory))
                Directory.Delete(ReportsDirectory, true);
            Directory.CreateDirectory(ReportsDirectory);

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

            // Explicitly fail the target if Docker Compose failed
            if (exitCode != 0)
            {
                Console.WriteLine($"Docker Compose exited with code: {exitCode}");
                throw new Exception($"E2E tests failed with exit code: {exitCode}");
            }
        });

    Target RunLocalServer => _ => _
        .Description("Run backend locally")
        .Executes(() =>
        {
            DotNetRun(s => s.SetProjectFile(ServerProject));
        });

    Target InstallClient => _ => _
        .Description("Install client dependencies if needed")
        .Executes(() =>
        {
            var packageLock = ClientDirectory / "package-lock.json";
            var nodeModules = ClientDirectory / "node_modules";

            if (!Directory.Exists(nodeModules) ||
                (File.Exists(packageLock) && File.GetLastWriteTime(packageLock) > Directory.GetLastWriteTime(nodeModules)))
            {
                Console.WriteLine("Installing/updating client dependencies...");
                NpmInstall(s => s
                    .SetProcessWorkingDirectory(ClientDirectory));
            }
            else
            {
                Console.WriteLine("Client dependencies are up to date.");
            }
        });

    Target RunLocalClient => _ => _
        .Description("Run client locally")
        .DependsOn(InstallClient)
        .Executes(() =>
        {
            Console.WriteLine($"Starting Vite dev server in {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("dev"));
        });

    Target DbReset => _ => _
        .Description("Delete local sqlite database (confirm or --force to skip)")
        .Executes(() =>
        {
            if (!Force)
            {
                Console.Write("Are you sure? [y/N] ");
                var response = Console.ReadLine();
                if (response?.ToLowerInvariant() != "y")
                {
                    Console.WriteLine("Operation cancelled");
                    return;
                }
            }

            Console.WriteLine($"Deleting {DatabaseFile}...");
            if (File.Exists(DatabaseFile))
            {
                File.Delete(DatabaseFile);
            }
            Console.WriteLine("Done.");
        });

    Target DbResetForce => _ => _
        .Description("Delete local sqlite database (no confirmation)")
        .Executes(() =>
        {
            Console.WriteLine($"Deleting {DatabaseFile}...");
            if (File.Exists(DatabaseFile))
            {
                File.Delete(DatabaseFile);
            }
            Console.WriteLine("Done.");
        });

    Target InstallDotnetToolLiquidReports => _ => _
        .Description("Install LiquidTestReports.Cli as a global dotnet tool")
        .Executes(() =>
        {
            try
            {
                Console.WriteLine("Updating LiquidTestReports.Cli global tool...");
                DotNetToolUpdate(s => s
                    .SetPackageName("LiquidTestReports.Cli")
                    .SetGlobal(true)
                    .SetProcessAdditionalArguments("--prerelease"));
            }
            catch
            {
                Console.WriteLine("Tool not found. Installing LiquidTestReports.Cli globally...");
                DotNetToolInstall(s => s
                    .SetPackageName("LiquidTestReports.Cli")
                    .SetGlobal(true)
                    .SetProcessAdditionalArguments("--prerelease"));
            }
        });
}