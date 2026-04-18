using System.Diagnostics;
using Nuke;
using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

public partial class Build
{
  internal const int ArchonApiPortDefault = 3090;
  internal const int ArchonWebPort = 5273;

  [Parameter("Path to local Archon checkout. Defaults to ../Archon relative to the conduit repo.")]
  internal readonly string? ArchonRoot;

  internal int ResolvedArchonApiPort
  {
    get
    {
      var envFile = ResolvedArchonRoot / ".env";
      if (envFile.FileExists())
      {
        foreach (var raw in envFile.ReadAllLines())
        {
          var line = raw.Trim();
          if (!line.StartsWith("PORT=", StringComparison.Ordinal))
          {
            continue;
          }
          var value = line.Substring("PORT=".Length).Trim().Trim('"', '\'');
          if (int.TryParse(value, out var port))
          {
            return port;
          }
        }
      }
      return ArchonApiPortDefault;
    }
  }

  internal string ArchonHealthUrl => $"http://localhost:{ResolvedArchonApiPort}/health";

  internal string ArchonWebUrl => $"http://localhost:{ArchonWebPort}";

  internal AbsolutePath ResolvedArchonRoot
  {
    get
    {
      if (!string.IsNullOrEmpty(ArchonRoot))
      {
        return (AbsolutePath)ArchonRoot;
      }

      // Climb out of .claude/worktrees/<name>/ so the default resolves to a
      // sibling of the main conduit checkout regardless of worktree nesting.
      var checkoutRoot = Constants.Worktree.IsMainCheckout(RootDirectory)
        ? RootDirectory
        : RootDirectory.Parent.Parent.Parent;
      return checkoutRoot.Parent / "Archon";
    }
  }

  internal AbsolutePath ArchonServerPidFile => PidDirectory / "archon-server.pid";

  internal AbsolutePath ArchonWebPidFile => PidDirectory / "archon-web.pid";

  internal AbsolutePath ArchonLogFile => RootDirectory / ".nuke" / "temp" / "archon.log";

  internal Target ArchonUp => _ => _
    .Description("Start local Archon server and web UI (:5273) as background processes")
    .Executes(() =>
    {
      if (!ResolvedArchonRoot.DirectoryExists())
      {
        throw new Exception(
          $"Archon checkout not found at {ResolvedArchonRoot}. " +
          $"Pass --archon-root <path> or place an Archon checkout at ../Archon.");
      }

      PidDirectory.CreateDirectory();
      ArchonLogFile.Parent.CreateDirectory();

      var bun = ResolveBunExecutable();
      Log.Debug("Resolved bun executable: {Bun}", bun);

      // bun's workspace filter spawns child shells; they need bun on PATH too.
      var bunDir = ((AbsolutePath)bun).Parent;
      var existingPath = System.Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
      var envVars = new Dictionary<string, string>
      {
        ["PATH"] = $"{bunDir}:{existingPath}",
      };

      CleanStaleArchonPid(ArchonServerPidFile, "server");
      CleanStaleArchonPid(ArchonWebPidFile, "web UI");

      Log.Information("Starting Archon server in background from {Root}...", ResolvedArchonRoot);
      var serverProcess = StartDetachedProcess(
        bun,
        "--filter @archon/server dev",
        ArchonLogFile,
        ResolvedArchonRoot,
        envVars);
      ArchonServerPidFile.WriteAllText(serverProcess.Id.ToString());
      Log.Information("Archon server started with PID: {PID}", serverProcess.Id);

      Log.Information("Waiting for Archon server to be healthy at {Url}...", ArchonHealthUrl);
      if (!WaitForHttpEndpoint(ArchonHealthUrl, timeoutSeconds: 120))
      {
        Log.Error("Archon server did not become healthy within the timeout period");
        KillProcess(serverProcess.Id);
        ArchonServerPidFile.DeleteFile();
        throw new Exception($"Archon server failed to become healthy at {ArchonHealthUrl}");
      }
      Log.Information("Archon server is healthy at {Url}", ArchonHealthUrl);

      Log.Information("Starting Archon web UI in background on port {Port}...", ArchonWebPort);
      var webProcess = StartDetachedProcess(
        bun,
        $"--filter @archon/web dev -- --port {ArchonWebPort}",
        ArchonLogFile,
        ResolvedArchonRoot,
        envVars);
      ArchonWebPidFile.WriteAllText(webProcess.Id.ToString());
      Log.Information("Archon web UI started with PID: {PID}", webProcess.Id);

      Log.Information("Waiting for Archon web UI to be available at {Url}...", ArchonWebUrl);
      if (!WaitForHttpEndpoint(ArchonWebUrl, timeoutSeconds: 60))
      {
        Log.Error("Archon web UI did not become available within the timeout period");
        KillProcess(webProcess.Id);
        ArchonWebPidFile.DeleteFile();
        throw new Exception($"Archon web UI failed to become available at {ArchonWebUrl}");
      }

      Log.Information("Archon is running:");
      Log.Information("  Server: {Url}", ArchonHealthUrl);
      Log.Information("  Web UI: {Url}", ArchonWebUrl);
      Log.Information("  Logs:   {File}", ArchonLogFile);
    });

  internal Target ArchonDown => _ => _
    .Description("Stop Archon server and web UI background processes started by ArchonUp")
    .Executes(() =>
    {
      StopArchonPid(ArchonWebPidFile, "web UI");
      StopArchonPid(ArchonServerPidFile, "server");

      System.Threading.Thread.Sleep(1000);
      if (IsHttpEndpointAvailable(ArchonHealthUrl))
      {
        throw new Exception($"Archon server is still accessible at {ArchonHealthUrl}");
      }

      Log.Information("Archon stopped successfully");
    });

  private void CleanStaleArchonPid(AbsolutePath pidFile, string label)
  {
    if (!pidFile.FileExists())
    {
      return;
    }

    var stalePid = int.Parse(pidFile.ReadAllText());
    try
    {
      Process.GetProcessById(stalePid);
      throw new Exception($"Archon {label} is already running (PID: {stalePid}). Run ArchonDown first.");
    }
    catch (ArgumentException)
    {
      Log.Information("Cleaning up stale Archon {Label} PID file (process {PID} no longer running)", label, stalePid);
      pidFile.DeleteFile();
    }
  }

  private void StopArchonPid(AbsolutePath pidFile, string label)
  {
    if (!pidFile.FileExists())
    {
      Log.Information("No Archon {Label} PID file found", label);
      return;
    }

    var pid = int.Parse(pidFile.ReadAllText());
    Log.Information("Stopping Archon {Label} (PID: {PID})...", label, pid);
    KillProcess(pid);
    pidFile.DeleteFile();
  }

  private static string ResolveBunExecutable()
  {
    var bunInstall = System.Environment.GetEnvironmentVariable("BUN_INSTALL");
    if (!string.IsNullOrEmpty(bunInstall))
    {
      var candidate = (AbsolutePath)bunInstall / "bin" / "bun";
      if (candidate.FileExists())
      {
        return candidate;
      }
    }

    var home = System.Environment.GetEnvironmentVariable("HOME");
    if (!string.IsNullOrEmpty(home))
    {
      var conventional = (AbsolutePath)home / ".bun" / "bin" / "bun";
      if (conventional.FileExists())
      {
        return conventional;
      }
    }

    return "bun";
  }
}
