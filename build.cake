// ==================================================================================== //
// CAKE BUILD SCRIPT - Equivalent to Makefile functionality
// ==================================================================================== //

using System.Text.RegularExpressions;

// ==================================================================================== //
// CONFIG
// ==================================================================================== //

var target = Argument("target", "Help");
var serverSolution = Argument("server-solution", "./App/Server/Server.sln");
var serverProject = Argument("server-project", "./App/Server/src/Server.Web/Server.Web.csproj");
var clientSolution = Argument("client-solution", "./App/Client");
var postmanComposeFile = Argument("postman-compose-file", "./Infra/Postman/docker-compose.yml");
var folder = EnvironmentVariable("FOLDER") ?? "";
var force = Argument("force", "0") == "1";

// ==================================================================================== //
// HELPERS
// ==================================================================================== //

Setup(context =>
{
    Information("Running Cake Build Script");
    Information($"Target: {target}");
});

private void Confirm(string message = "Are you sure? [y/N]")
{
    if (force) return;
    
    Console.Write($"{message} ");
    var response = Console.ReadLine();
    if (response?.ToLower() != "y")
    {
        throw new Exception("Operation cancelled by user");
    }
}

// ==================================================================================== //
// HELPERS - DOCUMENTATION
// ==================================================================================== //

Task("Help")
    .Description("List all the available tasks")
    .Does(() =>
{
    Information("Available tasks:");
    Information("");
    
    var tasks = new Dictionary<string, string>
    {
        { "Help", "list all the available tasks" },
        { "Confirm", "confirm before running a destructive action" },
        { "Lint-Server", "verify backend formatting & analyzers (no changes). Fails if issues found" },
        { "Lint-Make", "lint makefile (using original Makefile)" },
        { "Lint-Client", "lint client code" },
        { "Build-Server", "dotnet build (backend)" },
        { "Build-Client", "build client (frontend)" },
        { "Test-Server", "run backend tests" },
        { "Test-Server-Postman-Prep", "helper utility to prep for postman tests (reset db, stop any background backend, start backend, wait for it to be up)" },
        { "Test-Server-Postman", "run postman tests" },
        { "Test-Server-Postman-Auth", "run postman tests in the Auth folder" },
        { "Test-Server-Postman-Articles-Empty", "run postman tests in the ArticlesEmpty folder" },
        { "Test-Server-Postman-Article", "run postman tests in the Article folder" },
        { "Test-Server-Postman-Feed", "run postman tests in the FeedAndArticles folder" },
        { "Test-Server-Postman-Profiles", "run postman tests in the Profiles folder" },
        { "Test-Server-Ping", "ping backend to see if it's up (requires backend running in background)" },
        { "Test-Client", "run client tests" },
        { "Run-Local-Server", "run backend locally" },
        { "Run-Local-Server-Background", "run backend in the background (for local development)" },
        { "Run-Local-Server-Background-Stop", "stop background backend (for local development)" },
        { "Run-Local-Client", "run client locally" },
        { "Db-Reset", "delete local sqlite database (confirm or force=1 to skip)" },
        { "Db-Reset-Force", "delete local sqlite database (no confirmation)" }
    };
    
    var maxLength = tasks.Keys.Max(k => k.Length);
    
    foreach (var task in tasks.OrderBy(t => t.Key))
    {
        Information($"  {task.Key.PadRight(maxLength)}  {task.Value}");
    }
});

Task("Confirm")
    .Description("Confirm before running a destructive action")
    .Does(() =>
{
    Confirm();
});

// ==================================================================================== //
// LINTING
// ==================================================================================== //

Task("Lint-Server")
    .Description("Verify backend formatting & analyzers (no changes). Fails if issues found")
    .Does(() =>
{
    Information($"Running dotnet format (verify only) on {serverSolution} ...");
    
    var settings = new DotNetFormatSettings
    {
        VerifyNoChanges = true
    };
    
    DotNetFormat(serverSolution, settings);
});

Task("Lint-Make")
    .Description("Lint makefile (using original Makefile)")
    .Does(() =>
{
    // Delegate to original Makefile for consistency
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "lint/make",
        Timeout = 120000, // 2 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception("Makefile linting failed");
    }
    
    Information("Makefile linting passed.");
});

Task("Lint-Client")
    .Description("Lint client code")
    .Does(() =>
{
    Information("No client linting configured yet.");
});

// ==================================================================================== //
// BUILD
// ==================================================================================== //

Task("Build-Server")
    .Description("dotnet build (backend)")
    .Does(() =>
{
    DotNetBuild(serverSolution);
});

Task("Build-Client")
    .Description("Build client (frontend)")
    .Does(() =>
{
    Information("No client build configured yet.");
});

// ==================================================================================== //
// TEST
// ==================================================================================== //

Task("Test-Server")
    .Description("Run backend tests")
    .Does(() =>
{
    CreateDirectory("TestResults");
    
    var settings = new DotNetTestSettings
    {
        Loggers = new string[] { "trx;LogFileName=test-results.trx" },
        ResultsDirectory = "./TestResults"
    };
    
    DotNetTest(serverSolution, settings);
});

Task("Test-Server-Postman-Prep")
    .Description("Helper utility to prep for postman tests")
    .Does(() =>
{
    // Delegate to make for complex prep work
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman/prep",
        Timeout = 300000, // 5 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman prep failed with exit code {result}");
    }
});

Task("Test-Server-Postman")
    .Description("Run postman tests")
    .Does(() =>
{
    // Delegate to make for complex postman testing
    var environmentVariables = new Dictionary<string, string>();
    if (!string.IsNullOrEmpty(folder))
    {
        environmentVariables.Add("FOLDER", folder);
    }
    
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman",
        EnvironmentVariables = environmentVariables,
        Timeout = 600000, // 10 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman tests failed with exit code {result}");
    }
});

// Postman test variants with folder settings
Task("Test-Server-Postman-Auth")
    .Description("Run postman tests in the Auth folder")
    .Does(() =>
{
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman/auth",
        Timeout = 600000, // 10 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman Auth tests failed with exit code {result}");
    }
});

Task("Test-Server-Postman-Articles-Empty")
    .Description("Run postman tests in the ArticlesEmpty folder")
    .Does(() =>
{
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman/articles-empty",
        Timeout = 600000, // 10 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman ArticlesEmpty tests failed with exit code {result}");
    }
});

Task("Test-Server-Postman-Article")
    .Description("Run postman tests in the Article folder")
    .Does(() =>
{
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman/article",
        Timeout = 600000, // 10 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman Article tests failed with exit code {result}");
    }
});

Task("Test-Server-Postman-Feed")
    .Description("Run postman tests in the FeedAndArticles folder")
    .Does(() =>
{
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman/feed",
        Timeout = 600000, // 10 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman Feed tests failed with exit code {result}");
    }
});

Task("Test-Server-Postman-Profiles")
    .Description("Run postman tests in the Profiles folder")
    .Does(() =>
{
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/postman/profiles",
        Timeout = 600000, // 10 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Postman Profiles tests failed with exit code {result}");
    }
});

Task("Test-Server-Ping")
    .Description("Ping backend to see if it's up (requires backend running in background)")
    .Does(() =>
{
    // Delegate to make for complex ping logic
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "test/server/ping",
        Timeout = 120000, // 2 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception("Server ping failed");
    }
});

Task("Test-Client")
    .Description("Run client tests")
    .Does(() =>
{
    Information("No client tests configured yet.");
});

// ==================================================================================== //
// RUN-LOCAL
// ==================================================================================== //

Task("Run-Local-Server")
    .Description("Run backend locally")
    .Does(() =>
{
    DotNetRun(serverProject);
});

Task("Run-Local-Server-Background")
    .Description("Run backend in the background (for local development)")
    .Does(() =>
{
    // Delegate to make for complex background process management
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "run-local/server/background",
        Timeout = 120000, // 2 minutes in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        throw new Exception($"Failed to start background server with exit code {result}");
    }
});

Task("Run-Local-Server-Background-Stop")
    .Description("Stop background backend (for local development)")
    .Does(() =>
{
    // Delegate to make for complex process stopping
    var result = StartProcess("make", new ProcessSettings
    {
        Arguments = "run-local/server/background/stop",
        Timeout = 60000, // 1 minute in milliseconds
        WorkingDirectory = Context.Environment.WorkingDirectory
    });
    
    if (result != 0)
    {
        Warning("Failed to stop background server, but continuing...");
    }
});

Task("Run-Local-Client")
    .Description("Run client locally")
    .Does(() =>
{
    Information("No client run-local configured yet.");
});

// ==================================================================================== //
// DB
// ==================================================================================== //

Task("Db-Reset")
    .Description("Delete local sqlite database (confirm or force=1 to skip)")
    .Does(() =>
{
    if (!force)
    {
        Confirm();
    }
    
    Information("Deleting App/Server/src/Server.Web/database.sqlite ...");
    var dbFile = File("App/Server/src/Server.Web/database.sqlite");
    if (FileExists(dbFile))
    {
        DeleteFile(dbFile);
    }
    Information("Done.");
});

Task("Db-Reset-Force")
    .Description("Delete local sqlite database (no confirmation)")
    .Does(() =>
{
    Information("Deleting App/Server/src/Server.Web/database.sqlite ...");
    var dbFile = File("App/Server/src/Server.Web/database.sqlite");
    if (FileExists(dbFile))
    {
        DeleteFile(dbFile);
    }
    Information("Done.");
});

// ==================================================================================== //
// DEFAULT TARGET
// ==================================================================================== //

RunTarget(target);