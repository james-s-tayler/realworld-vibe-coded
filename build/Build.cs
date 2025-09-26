using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild
{
    public static int Main() => Execute<Build>(x => x.HelpTarget);

    // Directory paths matching Makefile config
    AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";
    AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";
    AbsolutePath ClientSolution => RootDirectory / "App" / "Client";
    AbsolutePath TestResultsDirectory => RootDirectory / "TestResults";
    AbsolutePath DatabasePath => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "database.sqlite";

    Target HelpTarget => _ => _
        .Description("List all the available tasks")
        .Executes(() =>
        {
            Console.WriteLine("Available targets:");
            Console.WriteLine("help                    List all the available tasks");
            Console.WriteLine("lint-server             Verify backend formatting & analyzers");
            Console.WriteLine("lint-make              Lint makefile");
            Console.WriteLine("lint-client            Lint client code");
            Console.WriteLine("build-server           Dotnet build (backend)");
            Console.WriteLine("build-client           Build client (frontend)");
            Console.WriteLine("test-server            Run backend tests");
            Console.WriteLine("test-client            Run client tests");
            Console.WriteLine("run-local-server       Run backend locally");
            Console.WriteLine("run-local-client       Run client locally");
            Console.WriteLine("reset-database         Delete local sqlite database");
        });

    // ==================================================================================== //
    // LINTING
    // ==================================================================================== //

    Target LintServer => _ => _
        .Description("Verify backend formatting & analyzers (no changes). Fails if issues found")
        .Executes(() =>
        {
            Console.WriteLine($"Running dotnet format (verify only) on {ServerSolution} ...");
            DotNetFormat(s => s
                .SetProject(ServerSolution)
                .SetVerifyNoChanges(true));
        });

    Target LintMake => _ => _
        .Description("Lint makefile")
        .Executes(() =>
        {
            // Simulate the Makefile linting logic
            var makefilePath = RootDirectory / "Makefile";
            if (!File.Exists(makefilePath))
            {
                Console.WriteLine("Error: Makefile not found");
                Environment.Exit(1);
            }

            var content = File.ReadAllText(makefilePath);
            var lines = content.Split('\n');

            // Check for # in HELP comments (equivalent to lint/make/help/hash)
            var helpLines = lines
                .Where(line => line.StartsWith("#HELP"))
                .Select(line => line.Length > 5 ? line.Substring(5) : "")
                .Where(line => line.Contains('#'))
                .Count();

            if (helpLines > 0)
            {
                Console.WriteLine($"Error ❌❌❌ {helpLines} #HELP comment(s) contains a # character. This is not allowed as it breaks the help target.");
                Environment.Exit(1);
            }

            // Check that each target has help docs (equivalent to lint/make/help/count)
            var helpCount = lines.Count(line => line.StartsWith("#HELP"));
            var targetLines = lines
                .Where(line => Regex.IsMatch(line, @"^[A-Za-z0-9_/.-]+:"))
                .Where(line => !line.StartsWith(".") && !line.Contains("%"))
                .Select(line => line.Split(':')[0])
                .Distinct()
                .ToList();

            var targetCount = targetLines.Count;

            if (helpCount != targetCount)
            {
                Console.WriteLine($"Error ❌❌❌ targets={targetCount} but #HELP={helpCount}. Every target needs a #HELP comment directly above it.");
                Console.WriteLine("Targets found:");
                foreach (var target in targetLines)
                {
                    Console.WriteLine($"  - {target}");
                }
                Environment.Exit(1);
            }

            Console.WriteLine("Makefile linting passed.");
        });

    Target LintClient => _ => _
        .Description("Lint client code")
        .Executes(() =>
        {
            Console.WriteLine("No client linting configured yet.");
        });

    // ==================================================================================== //
    // BUILD
    // ==================================================================================== //

    Target BuildServer => _ => _
        .Description("Dotnet build (backend)")
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(ServerSolution));
        });

    Target BuildClient => _ => _
        .Description("Build client (frontend)")
        .Executes(() =>
        {
            Console.WriteLine("No client build configured yet.");
        });

    // ==================================================================================== //
    // TEST
    // ==================================================================================== //

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
            Console.WriteLine("No client tests configured yet.");
        });

    // ==================================================================================== //
    // RUN-LOCAL
    // ==================================================================================== //

    Target RunLocalServer => _ => _
        .Description("Run backend locally")
        .Executes(() =>
        {
            DotNetRun(s => s
                .SetProjectFile(ServerProject));
        });

    Target RunLocalClient => _ => _
        .Description("Run client locally")
        .Executes(() =>
        {
            Console.WriteLine("No client run-local configured yet.");
        });

    // ==================================================================================== //
    // DB
    // ==================================================================================== //

    Target ResetDatabase => _ => _
        .Description("Delete local sqlite database")
        .Executes(() =>
        {
            Console.WriteLine($"Deleting {DatabasePath} ...");
            if (File.Exists(DatabasePath))
            {
                File.Delete(DatabasePath);
            }
            Console.WriteLine("Done.");
        });
}