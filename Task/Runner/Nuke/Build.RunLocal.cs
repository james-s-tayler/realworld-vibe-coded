using System.Diagnostics;
using System.Net;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  AbsolutePath PidDirectory => RootDirectory / ".nuke" / "pids";
  AbsolutePath DocsMcpPidFile => PidDirectory / "docs-mcp-server.pid";
  AbsolutePath NgrokPidFile => PidDirectory / "ngrok.pid";
  Target RunLocalServer => _ => _
    .Description("Run backend locally using Docker Compose with SQL Server and hot-reload")
    .DependsOn(DbResetForce)
    .Executes(() =>
    {
      Log.Information("Starting local development environment with Docker Compose...");

      var composeFile = RootDirectory / "Task" / "LocalDev" / "docker-compose.yml";

      ConsoleCancelEventHandler? handler = null;
      handler = (_, e) =>
      {
        // Don’t let NUKE abort the build on Ctrl+C; let docker handle it.
        e.Cancel = true;
        Log.Warning("Ctrl+C received; waiting for Docker to stop containers...");
      };
      Console.CancelKeyPress += handler;

      try
      {
        // Run docker-compose to start SQL Server and API with hot-reload
        Log.Information("Running Docker Compose for local development...");
        var args = $"compose -f {composeFile} up --build";
        var process = ProcessTasks.StartProcess("docker", args,
              workingDirectory: RootDirectory);
        process.WaitForExit();
      }
      finally
      {
        Console.CancelKeyPress -= handler;
        // Clean up containers when user stops the process
        Log.Information("Cleaning up Docker Compose resources...");
        var downArgs = $"compose -f {composeFile} down";
        var downProcess = ProcessTasks.StartProcess("docker", downArgs,
              workingDirectory: RootDirectory,
              logOutput: false,
              logInvocation: false);
        downProcess.WaitForExit();
        Log.Information("✓ Docker Compose resources cleaned up");
      }
    });

  Target RunLocalClient => _ => _
    .Description("Run client locally")
    .DependsOn(InstallClient)
    .Executes(() =>
    {
      Log.Information($"Starting Vite dev server in {ClientDirectory}");
      NpmRun(s => s
        .SetProcessWorkingDirectory(ClientDirectory)
        .SetCommand("dev"));
    });

  Target RunLocalDocsMcpServerUp => _ => _
    .Description("Start Docs MCP Server and ngrok in the background")
    .Executes(() =>
    {
      // Ensure PID directory exists
      PidDirectory.CreateDirectory();

      // Check if services are already running
      if (File.Exists(DocsMcpPidFile) || File.Exists(NgrokPidFile))
      {
        Log.Warning("Services may already be running. Run RunLocalDocsMcpServerDown first.");
        var existingFiles = new[] { DocsMcpPidFile, NgrokPidFile }
          .Where(f => File.Exists(f))
          .Select(f => f.ToString());
        Log.Warning("PID files found: {Files}", string.Join(", ", existingFiles));
        throw new Exception("Services may already be running. Clean up PID files first.");
      }

      // Install the latest version of docs-mcp-server
      Log.Information("Installing latest version of @arabold/docs-mcp-server...");
      try
      {
        NpmInstall(s => s
          .SetProcessWorkingDirectory(RootDirectory)
          .SetPackages("@arabold/docs-mcp-server@latest")
          .EnableGlobal());
        Log.Information("✓ @arabold/docs-mcp-server@latest installed");
      }
      catch (Exception ex)
      {
        Log.Warning("Failed to install @arabold/docs-mcp-server@latest: {Message}", ex.Message);
        Log.Warning("Continuing with existing version...");
      }

      // Start Docs MCP Server in the background
      Log.Information("Starting Docs MCP Server in the background...");
      var mcpProcess = StartBackgroundProcess("npx", "@arabold/docs-mcp-server");
      File.WriteAllText(DocsMcpPidFile, mcpProcess.Id.ToString());
      Log.Information("Docs MCP Server started with PID: {PID}", mcpProcess.Id);

      // Wait for the server to be available
      Log.Information("Waiting for Docs MCP Server to be available at http://127.0.0.1:6280...");
      if (!WaitForHttpEndpoint("http://127.0.0.1:6280", timeoutSeconds: 60))
      {
        Log.Error("Docs MCP Server did not become available within the timeout period");
        // Clean up the started process
        KillProcess(mcpProcess.Id);
        File.Delete(DocsMcpPidFile);
        throw new Exception("Docs MCP Server failed to start");
      }
      Log.Information("✓ Docs MCP Server is available at http://127.0.0.1:6280");

      // Check if ngrok is available
      if (!IsCommandAvailable("ngrok"))
      {
        Log.Warning("ngrok is not installed or not in PATH");
        Log.Warning("Install ngrok from https://ngrok.com/download");
        Log.Warning("Continuing without ngrok...");
        Log.Information("✓ Docs MCP Server started successfully (without ngrok)");
        Log.Information("  Docs MCP Server: http://127.0.0.1:6280");
        Log.Information("  PID files stored in: {PidDirectory}", PidDirectory);
        return;
      }

      // Start ngrok in the background
      Log.Information("Starting ngrok in the background...");
      var ngrokProcess = StartBackgroundProcess("ngrok", "http 6280 --url noncognizably-chartographical-fae.ngrok-free.app");
      File.WriteAllText(NgrokPidFile, ngrokProcess.Id.ToString());
      Log.Information("ngrok started with PID: {PID}", ngrokProcess.Id);

      // Give ngrok a moment to initialize
      Log.Information("Waiting for ngrok to initialize...");
      System.Threading.Thread.Sleep(3000);

      Log.Information("✓ Services started successfully");
      Log.Information("  Docs MCP Server: http://127.0.0.1:6280");
      Log.Information("  ngrok: https://noncognizably-chartographical-fae.ngrok-free.app");
      Log.Information("  PID files stored in: {PidDirectory}", PidDirectory);
    });

  Target RunLocalDocsMcpServerDown => _ => _
    .Description("Stop Docs MCP Server and ngrok background processes")
    .Executes(() =>
    {
      var errors = new List<string>();

      // Stop ngrok first (reverse order of startup)
      if (File.Exists(NgrokPidFile))
      {
        var ngrokPid = int.Parse(File.ReadAllText(NgrokPidFile));
        Log.Information("Stopping ngrok (PID: {PID})...", ngrokPid);
        try
        {
          KillProcess(ngrokPid);
          File.Delete(NgrokPidFile);
          Log.Information("✓ ngrok stopped");
        }
        catch (Exception ex)
        {
          var error = $"Failed to stop ngrok: {ex.Message}";
          Log.Error(error);
          errors.Add(error);
        }
      }
      else
      {
        Log.Warning("ngrok PID file not found: {File}", NgrokPidFile);
      }

      // Stop Docs MCP Server
      if (File.Exists(DocsMcpPidFile))
      {
        var mcpPid = int.Parse(File.ReadAllText(DocsMcpPidFile));
        Log.Information("Stopping Docs MCP Server (PID: {PID})...", mcpPid);
        try
        {
          KillProcess(mcpPid);
          File.Delete(DocsMcpPidFile);
          Log.Information("✓ Docs MCP Server stopped");
        }
        catch (Exception ex)
        {
          var error = $"Failed to stop Docs MCP Server: {ex.Message}";
          Log.Error(error);
          errors.Add(error);
        }
      }
      else
      {
        Log.Warning("Docs MCP Server PID file not found: {File}", DocsMcpPidFile);
      }

      // Verify services are stopped
      Log.Information("Verifying services are stopped...");
      System.Threading.Thread.Sleep(1000);

      if (IsHttpEndpointAvailable("http://127.0.0.1:6280"))
      {
        var error = "Docs MCP Server is still accessible at http://127.0.0.1:6280";
        Log.Error(error);
        errors.Add(error);
      }
      else
      {
        Log.Information("✓ Docs MCP Server is no longer accessible");
      }

      if (errors.Any())
      {
        throw new Exception($"Errors occurred during shutdown: {string.Join("; ", errors)}");
      }

      Log.Information("✓ All services stopped successfully");
    });

  private Process StartBackgroundProcess(string executable, string arguments)
  {
    var startInfo = new ProcessStartInfo
    {
      FileName = executable,
      Arguments = arguments,
      UseShellExecute = false,
      CreateNoWindow = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      RedirectStandardInput = false,
      WorkingDirectory = RootDirectory
    };

    var process = new Process { StartInfo = startInfo };

    // Redirect output to logger (but don't block on it)
    process.OutputDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        Log.Debug("[{Executable}] {Output}", executable, e.Data);
      }
    };
    process.ErrorDataReceived += (sender, e) =>
    {
      if (!string.IsNullOrEmpty(e.Data))
      {
        Log.Debug("[{Executable}] {Error}", executable, e.Data);
      }
    };

    process.Start();
    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    // Give the process a moment to fail if there's an immediate error
    System.Threading.Thread.Sleep(500);
    if (process.HasExited)
    {
      throw new Exception($"Process {executable} exited immediately with code {process.ExitCode}");
    }

    return process;
  }

  private bool WaitForHttpEndpoint(string url, int timeoutSeconds)
  {
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
    var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
    var attempts = 0;

    while (DateTime.Now < endTime)
    {
      attempts++;
      try
      {
        var response = client.GetAsync(url).Result;
        Log.Debug("HTTP check attempt {Attempt}: Status {Status}", attempts, response.StatusCode);
        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Debug("HTTP check attempt {Attempt} failed: {Message}", attempts, ex.InnerException?.Message ?? ex.Message);
      }

      System.Threading.Thread.Sleep(1000);
    }

    Log.Error("HTTP endpoint check failed after {Attempts} attempts over {Timeout} seconds", attempts, timeoutSeconds);
    return false;
  }

  private bool IsHttpEndpointAvailable(string url)
  {
    try
    {
      using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
      var response = client.GetAsync(url).Result;
      return response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound;
    }
    catch
    {
      return false;
    }
  }

  private void KillProcess(int pid)
  {
    try
    {
      var process = Process.GetProcessById(pid);
      process.Kill(entireProcessTree: true);
      process.WaitForExit(5000);
    }
    catch (ArgumentException)
    {
      // Process already exited
      Log.Debug("Process {PID} is not running", pid);
    }
    catch (Exception ex)
    {
      Log.Warning("Error killing process {PID}: {Message}", pid, ex.Message);
      throw;
    }
  }

  private bool IsCommandAvailable(string command)
  {
    try
    {
      var startInfo = new ProcessStartInfo
      {
        FileName = "which",
        Arguments = command,
        UseShellExecute = false,
        CreateNoWindow = true,
        RedirectStandardOutput = true
      };
      var process = Process.Start(startInfo);
      process?.WaitForExit();
      return process?.ExitCode == 0;
    }
    catch
    {
      return false;
    }
  }
}
