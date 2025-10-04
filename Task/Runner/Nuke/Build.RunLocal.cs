using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  Target RunLocalServer => _ => _
    .Description("Run backend locally")
    .Executes(() =>
    {
      DotNetRun(s => s.SetProjectFile(ServerProject));
    });

  Target RunLocalClient => _ => _
    .Description("Run client locally")
    .DependsOn(InstallClient)
    .Executes(() =>
    {
      Console.WriteLine($"Starting Vite dev server in {ClientDirectory}");
      NpmRun(s => s
        .SetProcessWorkingDirectory(ClientDirectory)
        .SetCommand("dev"));
    });
}
