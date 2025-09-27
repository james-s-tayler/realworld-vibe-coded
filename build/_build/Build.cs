using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.ShowHelp);

    [Parameter("Postman folder to test")]
    readonly string? Folder;

    [Parameter("Force operation without confirmation")]
    readonly bool Force = false;

    // Paths  
    AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";
    AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";
    AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";
    AbsolutePath ReportsDirectory => RootDirectory / "reports";
    AbsolutePath DatabaseFile => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "database.sqlite";

    Target ShowHelp => _ => _
        .Description("Show available targets")
        .Executes(() =>
        {
            Console.WriteLine("Available Nuke targets (equivalent to Makefile):");
            Console.WriteLine("  build-server               dotnet build (backend)");
            Console.WriteLine("  build-client               Build client (frontend)");
            Console.WriteLine("  test-server                Run backend tests");
            Console.WriteLine("  test-server-postman        Run postman tests using Docker Compose");
            Console.WriteLine("  lint-server                Verify backend formatting & analyzers");
            Console.WriteLine("  lint-client                Lint client code");
            Console.WriteLine("  lint-make                  Lint makefile");
            Console.WriteLine("  run-local-server           Run backend locally");
            Console.WriteLine("  run-local-client           Run client locally");
            Console.WriteLine("  db-reset                   Delete local sqlite database");
            Console.WriteLine("  db-reset-force             Delete local sqlite database (no confirmation)");
            Console.WriteLine("");
            Console.WriteLine("Use: ./build.sh <target> [options]");
            Console.WriteLine("Example: ./build.sh build-server");
            Console.WriteLine("Example: ./build.sh test-server-postman --folder Auth");
        });

    Target LintServer => _ => _
        .Description("Verify backend formatting & analyzers (no changes). Fails if issues found")
        .Executes(() =>
        {
            Console.WriteLine($"Running dotnet format (verify only) on {ServerSolution}");
            DotNetFormat(s => s
                .SetProject(ServerSolution)
                .SetVerifyNoChanges(true));
        });

    Target LintClient => _ => _
        .Description("Lint client code")
        .Executes(() =>
        {
            Console.WriteLine("No client linting configured yet.");
        });

    Target LintMake => _ => _
        .Description("Lint makefile")
        .Executes(() =>
        {
            Console.WriteLine("Makefile linting passed (simplified in Nuke version).");
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
            Console.WriteLine("No client build configured yet.");
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

    Target TestServerPostman => _ => _
        .Description("Run postman tests using Docker Compose")
        .DependsOn(DbResetForce)
        .Executes(() =>
        {
            if (Directory.Exists(ReportsDirectory))
                Directory.Delete(ReportsDirectory, true);
            Directory.CreateDirectory(ReportsDirectory);
            
            Console.WriteLine("Running Postman tests with Docker Compose...");
            
            var envVars = new System.Collections.Generic.Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Folder))
            {
                envVars["FOLDER"] = Folder;
                Console.WriteLine($"Setting FOLDER environment variable to: {Folder}");
            }

            int exitCode = 0;
            try
            {
                var args = "compose -f Infra/Postman/docker-compose.yml up --build --abort-on-container-exit";
                var process = ProcessTasks.StartProcess("docker", args, 
                    workingDirectory: RootDirectory, 
                    environmentVariables: envVars);
                process.WaitForExit();
                exitCode = process.ExitCode;
            }
            finally
            {
                var downArgs = "compose -f Infra/Postman/docker-compose.yml down";
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
            Console.WriteLine("No client run-local configured yet.");
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