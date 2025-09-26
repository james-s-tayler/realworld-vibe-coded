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
    AbsolutePath ReportsDirectory => RootDirectory / "reports";
    AbsolutePath PostmanComposeFile => RootDirectory / "Infra" / "Postman" / "docker-compose.yml";
    AbsolutePath DatabasePath => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "database.sqlite";

    Target HelpTarget => _ => _
        .Description("List all the available tasks")
        .Executes(() =>
        {
            Console.WriteLine("Available targets:");
            Console.WriteLine("help                         List all the available tasks");
            Console.WriteLine("lint-server                  Verify backend formatting & analyzers");
            Console.WriteLine("lint-make                   Lint makefile");
            Console.WriteLine("lint-client                 Lint client code");
            Console.WriteLine("build-server                Dotnet build (backend)");
            Console.WriteLine("build-client                Build client (frontend)");
            Console.WriteLine("test-server                 Run backend tests");
            Console.WriteLine("test-server-postman-prep     Helper utility to prep for postman tests");
            Console.WriteLine("test-server-postman         Run postman tests");
            Console.WriteLine("test-server-postman-auth    Run postman tests in the Auth folder");
            Console.WriteLine("test-server-postman-articles-empty  Run postman tests in the ArticlesEmpty folder");  
            Console.WriteLine("test-server-postman-article Run postman tests in the Article folder");
            Console.WriteLine("test-server-postman-feed    Run postman tests in the FeedAndArticles folder");
            Console.WriteLine("test-server-postman-profiles Run postman tests in the Profiles folder");
            Console.WriteLine("test-server-ping            Ping backend to see if it's up");
            Console.WriteLine("test-client                 Run client tests");
            Console.WriteLine("run-local-server            Run backend locally");
            Console.WriteLine("run-local-server-background Run backend in the background");
            Console.WriteLine("run-local-server-background-stop Stop background backend");
            Console.WriteLine("run-local-client            Run client locally");
            Console.WriteLine("reset-database              Delete local sqlite database");
            Console.WriteLine("reset-database-force        Delete local sqlite database (no confirmation)");
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

    // Postman Testing Targets

    Target TestServerPostmanPrep => _ => _
        .Description("Helper utility to prep for postman tests (reset db, stop any background backend, start backend, wait for it to be up)")
        .DependsOn(ResetDatabaseForce, RunLocalServerBackgroundStop, RunLocalServerBackground, TestServerPing);

    Target TestServerPostman => _ => _
        .Description("Run postman tests")
        .DependsOn(TestServerPostmanPrep)
        .Executes(() =>
        {
            if (Directory.Exists(ReportsDirectory))
                Directory.Delete(ReportsDirectory, true);
            Directory.CreateDirectory(ReportsDirectory);
            
            if (File.Exists(ReportsDirectory / "newman-report.json"))
            {
                File.Delete(ReportsDirectory / "newman-report.json");
            }
            
            try
            {
                var folder = Environment.GetEnvironmentVariable("FOLDER") ?? "";
                
                // Set up docker compose command with environment variable
                var processArgs = $"compose -f {PostmanComposeFile} up --abort-on-container-exit";
                
                // Set environment variable if provided and run docker compose
                var originalFolder = Environment.GetEnvironmentVariable("FOLDER");
                if (!string.IsNullOrEmpty(folder))
                {
                    Environment.SetEnvironmentVariable("FOLDER", folder);
                }
                
                try
                {
                    ProcessTasks.StartProcess("docker", processArgs, workingDirectory: RootDirectory)
                        .AssertWaitForExit()
                        .AssertZeroExitCode();
                }
                finally
                {
                    if (!string.IsNullOrEmpty(folder))
                    {
                        Environment.SetEnvironmentVariable("FOLDER", originalFolder);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Postman tests failed: {ex.Message}");
                throw;
            }
            finally
            {
                // Always try to stop the background server
                try
                {
                    ProcessTasks.StartProcess("pkill", "dotnet")
                        .AssertWaitForExit();
                }
                catch
                {
                    // Ignore errors, similar to Makefile
                }
            }
        });

    Target TestServerPostmanAuth => _ => _
        .Description("Run postman tests in the Auth folder")
        .Executes(() =>
        {
            Environment.SetEnvironmentVariable("FOLDER", "Auth");
            ProcessTasks.StartProcess("dotnet", "run --project build/_build.csproj -- TestServerPostman", workingDirectory: RootDirectory)
                .AssertWaitForExit()
                .AssertZeroExitCode();
        });

    Target TestServerPostmanArticlesEmpty => _ => _
        .Description("Run postman tests in the ArticlesEmpty folder")
        .Executes(() =>
        {
            Environment.SetEnvironmentVariable("FOLDER", "ArticlesEmpty");
            ProcessTasks.StartProcess("dotnet", "run --project build/_build.csproj -- TestServerPostman", workingDirectory: RootDirectory)
                .AssertWaitForExit()
                .AssertZeroExitCode();
        });

    Target TestServerPostmanArticle => _ => _
        .Description("Run postman tests in the Article folder")
        .Executes(() =>
        {
            Environment.SetEnvironmentVariable("FOLDER", "Article");
            ProcessTasks.StartProcess("dotnet", "run --project build/_build.csproj -- TestServerPostman", workingDirectory: RootDirectory)
                .AssertWaitForExit()
                .AssertZeroExitCode();
        });

    Target TestServerPostmanFeed => _ => _
        .Description("Run postman tests in the FeedAndArticles folder")
        .Executes(() =>
        {
            Environment.SetEnvironmentVariable("FOLDER", "FeedAndArticles");
            ProcessTasks.StartProcess("dotnet", "run --project build/_build.csproj -- TestServerPostman", workingDirectory: RootDirectory)
                .AssertWaitForExit()
                .AssertZeroExitCode();
        });

    Target TestServerPostmanProfiles => _ => _
        .Description("Run postman tests in the Profiles folder")
        .Executes(() =>
        {
            Environment.SetEnvironmentVariable("FOLDER", "Profiles");
            ProcessTasks.StartProcess("dotnet", "run --project build/_build.csproj -- TestServerPostman", workingDirectory: RootDirectory)
                .AssertWaitForExit()
                .AssertZeroExitCode();
        });

    Target TestServerPing => _ => _
        .Description("Ping backend to see if it's up (requires backend running in background)")
        .Executes(() =>
        {
            var timeout = 60;
            var url = "https://localhost:57679/swagger/index.html";
            
            for (int i = 1; i <= timeout; i++)
            {
                Console.WriteLine($"Pinging {url} (attempt {i} of {timeout}) ...");
                try
                {
                    // Use curl command similar to Makefile
                    var result = ProcessTasks.StartProcess("curl", 
                        $"-k -s -o /dev/null -w \"%{{http_code}}\" {url}")
                        .AssertWaitForExit();
                    
                    if (result.ExitCode == 0)
                    {
                        return;
                    }
                }
                catch
                {
                    // Continue trying
                }
                
                if (i == timeout)
                {
                    Console.WriteLine("Backend ping timeout");
                    Environment.Exit(1);
                }
                
                System.Threading.Thread.Sleep(1000);
            }
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

    Target RunLocalServerBackground => _ => _
        .Description("Run backend in the background (for local development)")
        .Executes(() =>
        {
            // Start the server in the background exactly like Makefile does: dotnet run &
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project {ServerProject}",
                WorkingDirectory = RootDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            var process = System.Diagnostics.Process.Start(startInfo);
            
            Console.WriteLine($"Started server in background with PID: {process.Id}");
            // Give the process time to start up properly  
            System.Threading.Thread.Sleep(5000);
        });

    Target RunLocalServerBackgroundStop => _ => _
        .Description("Stop background backend (for local development)")
        .Executes(() =>
        {
            Console.WriteLine("Killing Server.Web ...");
            try
            {
                ProcessTasks.StartProcess("pkill", "dotnet")
                    .AssertWaitForExit();
            }
            catch
            {
                // Ignore errors, similar to Makefile -pkill dotnet || true
            }
            Console.WriteLine("Done.");
        });

    // ==================================================================================== //
    // DB
    // ==================================================================================== //

    Target ResetDatabase => _ => _
        .Description("Delete local sqlite database (confirm or FORCE=1 to skip)")
        .Executes(() =>
        {
            if (Environment.GetEnvironmentVariable("FORCE") != "1")
            {
                Console.WriteLine("Are you sure? [y/N]");
                var input = Console.ReadLine()?.Trim().ToLower();
                if (input != "y" && input != "yes")
                {
                    Console.WriteLine("Operation cancelled.");
                    Environment.Exit(1);
                }
            }
            
            Console.WriteLine($"Deleting {DatabasePath} ...");
            if (File.Exists(DatabasePath))
            {
                File.Delete(DatabasePath);
            }
            Console.WriteLine("Done.");
        });

    Target ResetDatabaseForce => _ => _
        .Description("Delete local sqlite database (no confirmation)")
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