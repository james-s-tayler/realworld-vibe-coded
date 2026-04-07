using System.Diagnostics;
using System.Net;
using Nuke;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  internal AbsolutePath PidDirectory => RootDirectory / ".nuke" / "pids";

  internal AbsolutePath DocsMcpPidFile => PidDirectory / "docs-mcp-server.pid";

  internal AbsolutePath DockerComposePublish => TaskLocalDevDirectory / "docker-compose.publish.yml";

  internal Target RunLocalPublish => _ => _
    .Description("Run backend locally using Docker Compose with published artifact")
    .DependsOn(RunLocalDependencies)
    .DependsOn(BuildServerPublish)
    .DependsOn(PathsCleanDirectories)
    .Executes(() =>
    {
      Log.Information("Starting local development environment with Docker Compose (published artifact)...");
      LogWorktreeInfo();

      var envVars = new Dictionary<string, string>(GetWorktreeEnvVars())
      {
        ["DOCKER_BUILDKIT"] = "1",
      };

      var detached = Agent ? " -d" : string.Empty;
      Log.Information("Running Docker Compose for local development with published artifact...");
      var args = $"compose -f {DockerComposePublish} -p {ScopedProjectName(Constants.Docker.Projects.App)} up --build{detached}";
      var process = ProcessTasks.StartProcess(
            "docker",
            args,
            workingDirectory: RootDirectory,
            environmentVariables: envVars);
      process.WaitForExit();
    });

  internal Target RunLocalPublishDown => _ => _
    .Description("Stop backend Docker Compose containers")
    .Executes(() =>
    {
      Log.Information("Stopping local published app containers...");
      var envVars = GetWorktreeEnvVars();
      var args = $"compose -f {DockerComposePublish} -p {ScopedProjectName(Constants.Docker.Projects.App)} down";
      var process = ProcessTasks.StartProcess(
        "docker",
        args,
        workingDirectory: RootDirectory,
        environmentVariables: envVars);
      process.WaitForExit();
    });

  internal Target RunLocalDependencies => _ => _
    .Description("Run dev dependencies")
    .DependsOn(InstallDockerNetwork)
    .Executes(() =>
    {
      Log.Information("Starting dev dependencies");

      var envVars = new Dictionary<string, string>(GetWorktreeEnvVars())
      {
        ["DOCKER_BUILDKIT"] = "1",
      };

      try
      {
        Log.Information("Running Docker Compose for dev dependencies...");
        var args = $"compose -f {DockerComposeDependencies} -p {ScopedProjectName(Constants.Docker.Projects.DevDependencies)} up -d";
        var process = ProcessTasks.StartProcess(
          "docker",
          args,
          workingDirectory: RootDirectory,
          environmentVariables: envVars);
        process.WaitForExit();
      }
      catch (Exception ex)
      {
        Log.Error(ex, "An error occurred while trying to start dev dependencies: {Message}", ex.Message);
        throw;
      }
    });

  internal Target RunLocalDependenciesDown => _ => _
    .Description("Stop dev dependencies")
    .Executes(() =>
    {
      Log.Information("Stopping dev dependencies");
      var envVars = GetWorktreeEnvVars();
      var args = $"compose -f {DockerComposeDependencies} -p {ScopedProjectName(Constants.Docker.Projects.DevDependencies)} down";
      var process = ProcessTasks.StartProcess(
        "docker",
        args,
        workingDirectory: RootDirectory,
        environmentVariables: envVars);
      process.WaitForExit();
    });

  internal Target RunLocalServer => _ => _
    .Description("Run backend in source-mounted Docker container for hot-reload development")
    .DependsOn(RunLocalDependencies)
    .DependsOn(PathsCleanDirectories)
    .Executes(() =>
    {
      Log.Information("Starting backend server in source-mounted Docker container...");
      LogWorktreeInfo();

      var envVars = new Dictionary<string, string>(GetWorktreeEnvVars())
      {
        ["DOCKER_BUILDKIT"] = "1",
      };

      var args = $"compose -f {DockerComposeServer} -p {ScopedProjectName(Constants.Docker.Projects.Server)} up --build -d";
      var process = ProcessTasks.StartProcess(
        "docker",
        args,
        workingDirectory: RootDirectory,
        environmentVariables: envVars);
      process.WaitForExit();

      var offset = Constants.Worktree.GetPortOffset(RootDirectory);
      var httpsPort = 5001 + offset;
      var healthUrl = $"https://localhost:{httpsPort}/health/ready";
      Log.Information("Waiting for backend to be healthy at {Url}...", healthUrl);
      if (!WaitForHttpsEndpoint(healthUrl, timeoutSeconds: 300))
      {
        throw new Exception($"Backend did not become healthy at {healthUrl} within timeout");
      }

      Log.Information("✓ Backend server is healthy and ready");
    });

  internal Target RunLocalServerDown => _ => _
    .Description("Stop backend source-mounted Docker container")
    .Executes(() =>
    {
      Log.Information("Stopping backend server container...");
      var envVars = GetWorktreeEnvVars();
      var args = $"compose -f {DockerComposeServer} -p {ScopedProjectName(Constants.Docker.Projects.Server)} down";
      var process = ProcessTasks.StartProcess(
        "docker",
        args,
        workingDirectory: RootDirectory,
        environmentVariables: envVars);
      process.WaitForExit();
    });

  internal Target RunLocalHotReload => _ => _
    .Description("Run backend (Docker) + frontend (Vite HMR) for hot-reload development")
    .DependsOn(RunLocalServer)
    .DependsOn(RunLocalClient);

  internal Target RunLocalHotReloadDown => _ => _
    .Description("Stop backend container and Vite dev server")
    .DependsOn(RunLocalServerDown)
    .DependsOn(RunLocalClientDown);

  internal Target RunLocalClient => _ => _
    .Description("Run client locally")
    .DependsOn(InstallClient)
    .After(RunLocalServer)
    .Executes(() =>
    {
      var offset = Constants.Worktree.GetPortOffset(RootDirectory);
      var apiPort = 5001 + offset;
      var vitePort = 5173 + offset;
      Log.Information("Starting Vite dev server on port {VitePort} in {ClientDirectory} (API proxy → https://localhost:{ApiPort})", vitePort, ClientDirectory, apiPort);
      NpmRun(s => s
        .SetProcessWorkingDirectory(ClientDirectory)
        .SetCommand("dev")
        .SetProcessEnvironmentVariable("API_PROXY_TARGET", $"https://localhost:{apiPort}")
        .SetProcessEnvironmentVariable("VITE_DEV_PORT", $"{vitePort}"));
    });

  internal Target RunLocalClientDown => _ => _
    .Description("Stop Vite dev server")
    .Executes(() =>
    {
      var offset = Constants.Worktree.GetPortOffset(RootDirectory);
      var vitePort = 5173 + offset;
      Log.Information("Stopping Vite dev server on port {Port}...", vitePort);
      KillProcessOnPort(vitePort);
    });

  internal Target RunLocalDocsMcpServerUp => _ => _
    .Description("Start Docs MCP Server in the background")
    .Executes(() =>
    {
      // Ensure PID directory exists
      PidDirectory.CreateDirectory();

      // Check if server is already running
      if (DocsMcpPidFile.FileExists())
      {
        Log.Warning("Docs MCP Server may already be running. Run RunLocalDocsMcpServerDown first.");
        throw new Exception("Docs MCP Server may already be running. Clean up PID file first.");
      }

      // Start Docs MCP Server in the background
      Log.Information("Starting Docs MCP Server in the background...");
      var mcpProcess = StartBackgroundProcess("npx", $"--yes @arabold/docs-mcp-server@latest server --protocol http --port {DocsMcpPort}");
      DocsMcpPidFile.WriteAllText(mcpProcess.Id.ToString());
      Log.Information("Docs MCP Server started with PID: {PID}", mcpProcess.Id);

      // Wait for the server to be available
      var mcpUrl = $"http://127.0.0.1:{DocsMcpPort}";
      Log.Information("Waiting for Docs MCP Server to be available at {Url}...", mcpUrl);
      if (!WaitForHttpEndpoint(mcpUrl, timeoutSeconds: 60))
      {
        Log.Error("Docs MCP Server did not become available within the timeout period");

        // Clean up the started process
        KillProcess(mcpProcess.Id);
        DocsMcpPidFile.DeleteFile();
        throw new Exception("Docs MCP Server failed to start - try run npx @arabold/docs-mcp-server@latest");
      }

      Log.Information("✓ Docs MCP Server started successfully at {Url}", mcpUrl);
      Log.Information("  PID file stored in: {PidDirectory}", PidDirectory);
    });

  internal Target RunLocalDocsMcpServerDown => _ => _
    .Description("Stop Docs MCP Server background process")
    .Executes(() =>
    {
      var errors = new List<string>();

      if (DocsMcpPidFile.FileExists())
      {
        var mcpPid = int.Parse(DocsMcpPidFile.ReadAllText());
        Log.Information("Stopping Docs MCP Server (PID: {PID})...", mcpPid);
        try
        {
          KillProcess(mcpPid);
          DocsMcpPidFile.DeleteFile();
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

      if (IsHttpEndpointAvailable($"http://127.0.0.1:{DocsMcpPort}"))
      {
        var error = $"Docs MCP Server is still accessible at http://127.0.0.1:{DocsMcpPort}";
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
      WorkingDirectory = RootDirectory,
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

  private bool WaitForHttpsEndpoint(string url, int timeoutSeconds)
  {
    var handler = new HttpClientHandler
    {
      ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
    };
    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) };
    var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
    var attempts = 0;

    while (DateTime.Now < endTime)
    {
      attempts++;
      try
      {
        var response = client.GetAsync(url).Result;
        Log.Debug("HTTPS check attempt {Attempt}: Status {Status}", attempts, response.StatusCode);
        if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.NotFound)
        {
          return true;
        }
      }
      catch (Exception ex)
      {
        Log.Debug("HTTPS check attempt {Attempt} failed: {Message}", attempts, ex.InnerException?.Message ?? ex.Message);
      }

      System.Threading.Thread.Sleep(2000);
    }

    Log.Error("HTTPS endpoint check failed after {Attempts} attempts over {Timeout} seconds", attempts, timeoutSeconds);
    return false;
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

  private void KillProcessOnPort(int port)
  {
    try
    {
      var process = ProcessTasks.StartProcess(
        "lsof",
        $"-ti :{port}",
        logOutput: false);
      process.WaitForExit();

      if (process.ExitCode == 0)
      {
        var pids = process.Output
          .Select(o => o.Text.Trim())
          .Where(s => !string.IsNullOrEmpty(s));

        foreach (var pidStr in pids)
        {
          if (int.TryParse(pidStr, out var pid))
          {
            KillProcess(pid);
            Log.Information("✓ Killed process {PID} on port {Port}", pid, port);
          }
        }
      }
      else
      {
        Log.Information("No process found on port {Port}", port);
      }
    }
    catch (Exception ex)
    {
      Log.Warning("Could not find process on port {Port}: {Message}", port, ex.Message);
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
}
