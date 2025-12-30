using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  internal Target BuildServer => _ => _
    .Description("dotnet build (backend)")
    .Executes(() =>
    {
      DotNetBuild(s => s.SetProjectFile(ServerSolution));
    });

  internal Target BuildServerPublish => _ => _
    .Description("Publish App/Server for linux-x64 in Release configuration with App/Client/dist in wwwroot")
    .DependsOn(BuildClient)
    .Executes(() =>
    {
      if (SkipPublish)
      {
        Log.Information("Already published - skipping");
        return;
      }

      PublishDirectory.CreateOrCleanDirectory();

      Log.Information($"Publishing server to {PublishDirectory}");
      DotNetPublish(s => s
        .SetProject(ServerProject)
        .SetConfiguration("Release")
        .SetRuntime("linux-x64")
        .SetOutput(PublishDirectory));

      // Copy client dist to publish/wwwroot
      Log.Information($"Copying client dist from {ClientDistDirectory} to {PublishWwwRootDirectory}");

      // Use AbsolutePath GlobFiles to get all files recursively
      var files = ClientDistDirectory.GlobFiles("**/*");
      foreach (var file in files)
      {
        var relativePath = ClientDistDirectory.GetRelativePathTo(file);
        var targetPath = PublishWwwRootDirectory / relativePath;
        var targetDir = targetPath.Parent;

        targetDir.CreateDirectory();
        file.Copy(targetPath, ExistsPolicy.FileOverwrite);
      }
    });

  internal Target BuildClient => _ => _
    .Description("Build client (frontend)")
    .DependsOn(InstallClient)
    .Executes(() =>
    {
      if (SkipPublish)
      {
        Log.Information("Already published - skipping");
        return;
      }

      Console.WriteLine($"Building client in {ClientDirectory}");
      NpmRun(s => s
        .SetProcessWorkingDirectory(ClientDirectory)
        .SetCommand("build"));
    });

  internal Target BuildRoslynMcp => _ => _
    .Description("Build RoslynMCP server")
    .Executes(() =>
    {
      Log.Information($"Building RoslynMCP project at {RoslynMcpProject}");
      DotNetBuild(s => s
        .SetProjectFile(RoslynMcpProject)
        .SetConfiguration("Debug"));
    });
}
