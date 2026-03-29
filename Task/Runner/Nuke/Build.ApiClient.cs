using Nuke.Common;
using Nuke.Common.Tooling;
using Serilog;

public partial class Build
{
  internal Target BuildGenerateApiClient => _ => _
    .Description("Generate TypeScript API client from FastEndpoints using Kiota")
    .DependsOn(BuildServer)
    .Executes(() =>
    {
      Log.Information("Generating TypeScript API client from {ServerProject}", ServerProject);

      var result = ProcessTasks.StartProcess(
        "dotnet",
        $"run --project {ServerProject} -- --generateclients true",
        RootDirectory);
      result.AssertWaitForExit();
      result.AssertZeroExitCode();
    });

  internal Target LintApiClientVerify => _ => _
    .Description("Verify generated API client is in sync with server endpoints")
    .DependsOn(BuildGenerateApiClient)
    .Executes(() =>
    {
      Log.Information("Checking for API client drift in {ClientApiGeneratedDirectory}", ClientApiGeneratedDirectory);

      var result = ProcessTasks.StartProcess(
        "git",
        $"diff --exit-code -- {ClientApiGeneratedDirectory}",
        RootDirectory);
      result.AssertWaitForExit();

      if (result.ExitCode != 0)
      {
        throw new System.Exception(
          "Generated API client is out of sync with server endpoints. " +
          "Run './build.sh BuildGenerateApiClient' and commit the changes.");
      }

      Log.Information("Generated API client is in sync with server endpoints");
    });
}
