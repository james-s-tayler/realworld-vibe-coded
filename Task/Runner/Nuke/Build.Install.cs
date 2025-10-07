using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Npm;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using static Nuke.Common.Tools.Npm.NpmTasks;

public partial class Build
{
  Target InstallClient => _ => _
      .Description("Run npm ci if node_modules is missing or package-lock.json is newer")
      .Executes(() =>
      {
        var packageLock = ClientDirectory / "package-lock.json";
        var nodeModules = ClientDirectory / "node_modules";

        if (!Directory.Exists(nodeModules) ||
              (File.Exists(packageLock) && File.GetLastWriteTime(packageLock) > Directory.GetLastWriteTime(nodeModules)))
        {
          Log.Information("Installing/updating client dependencies...");
          NpmCi(s => s
                .SetProcessWorkingDirectory(ClientDirectory));
        }
        else
        {
          Log.Information("Client dependencies are up to date.");
        }
      });

  Target InstallGitHooks => _ => _
    .Description("Install git hooks from .husky")
    .Executes(() =>
    {
      NpmCi();
      NpmRun(s => s
        .SetProcessWorkingDirectory(RootDirectory)
        .SetCommand("prepare"));
    });

  Target InstallDotnetToolLiquidReports => _ => _
      .Description("Install LiquidTestReports.Cli as a global dotnet tool")
      .Executes(() =>
      {
        try
        {
          Log.Information("Updating LiquidTestReports.Cli global tool...");
          DotNetToolUpdate(s => s
                .SetPackageName("LiquidTestReports.Cli")
                .SetGlobal(true)
                .SetProcessAdditionalArguments("--prerelease"));
        }
        catch
        {
          Log.Information("Tool not found. Installing LiquidTestReports.Cli globally...");
          DotNetToolInstall(s => s
                .SetPackageName("LiquidTestReports.Cli")
                .SetGlobal(true)
                .SetProcessAdditionalArguments("--prerelease"));
        }
      });

  Target InstallDotnetToolDotnetCoverage => _ => _
      .Description("Install dotnet-coverage as a global dotnet tool")
      .Executes(() =>
      {
        try
        {
          Log.Information("Updating dotnet-coverage global tool...");
          DotNetToolUpdate(s => s
                .SetPackageName("dotnet-coverage")
                .SetGlobal(true));
        }
        catch
        {
          Log.Information("Tool not found. Installing dotnet-coverage globally...");
          DotNetToolInstall(s => s
                .SetPackageName("dotnet-coverage")
                .SetGlobal(true));
        }
      });
}
