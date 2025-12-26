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
  internal Target InstallClient => _ => _
      .Description("Run npm ci if node_modules is missing or package-lock.json is newer")
      .Executes(() =>
      {
        if (SkipPublish)
        {
          Log.Information("Already published - skipping");
          return;
        }

        var packageLock = ClientDirectory / "package-lock.json";
        var nodeModules = ClientDirectory / "node_modules";

        if (!nodeModules.DirectoryExists() ||
              (packageLock.FileExists() && packageLock.ToFileInfo().LastWriteTime > nodeModules.ToDirectoryInfo().LastWriteTime))
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

  internal Target InstallGitHooks => _ => _
    .Description("Install git hooks from .husky")
    .Executes(() =>
    {
      NpmCi();
      NpmRun(s => s
        .SetProcessWorkingDirectory(RootDirectory)
        .SetCommand("prepare"));
    });

  internal Target InstallDotnetToolLiquidReports => _ => _
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

  internal Target InstallDotnetToolEf => _ => _
      .Description("Install dotnet-ef as a global dotnet tool")
      .Executes(() =>
      {
        try
        {
          Log.Information("Updating dotnet-ef global tool...");
          DotNetToolUpdate(s => s
                .SetPackageName("dotnet-ef")
                .SetGlobal(true)
                .SetVersion("10.*"));
        }
        catch
        {
          Log.Information("Tool not found. Installing dotnet-ef globally...");
          DotNetToolInstall(s => s
                .SetPackageName("dotnet-ef")
                .SetGlobal(true)
                .SetVersion("9.*"));
        }
      });
}
