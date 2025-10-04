using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
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
}
