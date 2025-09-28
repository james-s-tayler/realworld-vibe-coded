using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public class Build : NukeBuild
{
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
    AbsolutePath E2eTestsProject => RootDirectory / "Test" / "e2e" / "E2eTests" / "E2eTests.csproj";
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
        .Executes(() =>
        {
            Console.WriteLine($"Running ESLint on {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("lint"));
        });

    Target LintClientFix => _ => _
        .Description("Fix client code formatting and style issues automatically")
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
        .Executes(() =>
        {
            Console.WriteLine($"Building client in {ClientDirectory}");
            NpmRun(s => s
                .SetProcessWorkingDirectory(ClientDirectory)
                .SetCommand("build"));
        });

    Target TestServer => _ => _
        .Description("Run backend tests")
        .Executes(() =>
        {
            if (Directory.Exists(TestResultsDirectory))
                Directory.Delete(TestResultsDirectory, true);
            Directory.CreateDirectory(TestResultsDirectory);

            DotNetTest(s => s
                .SetProjectFile(ServerSolution)
                .SetLoggers("trx;LogFileName=test-results.trx")
                .SetResultsDirectory(TestResultsDirectory));
        });

    Target TestClient => _ => _
        .Description("Run client tests")
        .Executes(() =>
        {
            Console.WriteLine($"Running client tests in {ClientDirectory}");
            // Note: Vite starter doesn't include tests by default, this is a placeholder
            Console.WriteLine("No client tests configured yet. Add Vitest or Jest to enable client testing.");
        });

    Target TestE2e => _ => _
        .Description("Run end-to-end tests using Playwright")
        .Executes(() =>
        {
            if (Directory.Exists(TestResultsDirectory))
                Directory.Delete(TestResultsDirectory, true);
            Directory.CreateDirectory(TestResultsDirectory);

            Console.WriteLine($"Running Playwright e2e tests from {E2eTestsProject}");
            
            // Install Playwright browsers first
            var playwrightScript = E2eTestsProject.Parent / "E2eTests" / "bin" / "Debug" / "net9.0" / "playwright.ps1";
            if (File.Exists(playwrightScript))
            {
                Console.WriteLine("Installing Playwright browsers...");
                ProcessTasks.StartProcess("pwsh", $"{playwrightScript} install chromium").WaitForExit();
            }

            DotNetTest(s => s
                .SetProjectFile(E2eTestsProject)
                .SetLoggers("trx;LogFileName=e2e-test-results.trx")
                .SetResultsDirectory(TestResultsDirectory));
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

    Target RunLocalServer => _ => _
        .Description("Run backend locally")
        .Executes(() =>
        {
            DotNetRun(s => s.SetProjectFile(ServerProject));
        });

    Target RunLocalClient => _ => _
        .Description("Run client locally")
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
}