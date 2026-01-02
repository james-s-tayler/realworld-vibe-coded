using Nuke.Common;
using Nuke.Common.IO;
using Serilog;

public partial class Build
{
  // Paths

  #region App/Server

  internal AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";

  internal AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";

  internal AbsolutePath ServerInfrastructureProject => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Server.Infrastructure.csproj";

  internal AbsolutePath MigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations";

  internal AbsolutePath IdempotentScriptPath => MigrationsDirectory / "idempotent.sql";

  internal AbsolutePath TenantStoreMigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "TenantStoreMigrations";

  internal AbsolutePath TenantStoreIdempotentScriptPath => TenantStoreMigrationsDirectory / "idempotent.sql";

  #endregion

  #region App/Client

  internal AbsolutePath ClientDirectory => RootDirectory / "App" / "Client";

  internal AbsolutePath ClientDistDirectory => ClientDirectory / "dist";

  #endregion

  #region Publish

  internal AbsolutePath PublishDirectory => RootDirectory / "publish";

  internal AbsolutePath PublishWwwRootDirectory => PublishDirectory / "wwwroot";

  #endregion

  #region Task

  internal AbsolutePath TaskRunnerDirectory => RootDirectory / "Task" / "Runner";

  internal AbsolutePath TaskLocalDevDirectory => RootDirectory / "Task" / "LocalDev";

  internal AbsolutePath FlowPilotDirectory => RootDirectory / "Task" / "FlowPilot";

  internal AbsolutePath FlowPilotSolution => FlowPilotDirectory / "FlowPilot.sln";

  internal AbsolutePath FlowPilotCliProject => FlowPilotDirectory / "FlowPilot.Cli" / "FlowPilot.Cli.csproj";

  internal AbsolutePath FlowPilotTestProject => FlowPilotDirectory / "FlowPilot.Tests" / "FlowPilot.Tests.csproj";

  internal AbsolutePath RoslynMcpProject => RootDirectory / "Task" / "McpServers" / "roslyn-mcp" / "RoslynMCP" / "RoslynMCP.csproj";

  internal AbsolutePath LocalNuGetFeedDirectory => RootDirectory / ".local-nuget";

  #endregion

  #region Reports
  internal AbsolutePath ReportsServerDirectory => RootDirectory / "Reports" / "Server";

  internal AbsolutePath ReportsServerResultsDirectory => RootDirectory / "Reports" / "Server" / "Results";

  internal AbsolutePath ReportsServerArtifactsDirectory => RootDirectory / "Reports" / "Server" / "Artifacts";

  internal AbsolutePath ReportsClientDirectory => RootDirectory / "Reports" / "Client";

  internal AbsolutePath ReportsClientResultsDirectory => RootDirectory / "Reports" / "Client" / "Results";

  internal AbsolutePath ReportsClientArtifactsDirectory => RootDirectory / "Reports" / "Client" / "Artifacts";

  internal AbsolutePath ReportsTestE2eDirectory => RootDirectory / "Reports" / "Test" / "e2e";

  internal AbsolutePath ReportsTestE2eResultsDirectory => RootDirectory / "Reports" / "Test" / "e2e" / "Results";

  internal AbsolutePath ReportsTestE2eArtifactsDirectory => RootDirectory / "Reports" / "Test" / "e2e" / "Artifacts";

  internal AbsolutePath ReportsTestPostmanDirectory => RootDirectory / "Reports" / "Test" / "Postman";

  internal AbsolutePath ReportsTestPostmanArticlesEmptyDirectory => ReportsTestPostmanDirectory / "ArticlesEmpty";

  internal AbsolutePath ReportsTestPostmanAuthDirectory => ReportsTestPostmanDirectory / "Auth";

  internal AbsolutePath ReportsTestPostmanProfilesDirectory => ReportsTestPostmanDirectory / "Profiles";

  internal AbsolutePath ReportsTestPostmanFeedAndArticlesDirectory => ReportsTestPostmanDirectory / "FeedAndArticles";

  internal AbsolutePath ReportsTestPostmanArticleDirectory => ReportsTestPostmanDirectory / "Article";

  internal AbsolutePath ReportsFlowPilotDirectory => RootDirectory / "Reports" / "FlowPilot";

  internal AbsolutePath ReportsFlowPilotResultsDirectory => RootDirectory / "Reports" / "FlowPilot" / "Results";

  internal AbsolutePath ReportsFlowPilotArtifactsDirectory => RootDirectory / "Reports" / "FlowPilot" / "Artifacts";
  #endregion

  #region Logs
  internal AbsolutePath LogsDirectory => RootDirectory / "Logs";

  internal AbsolutePath LogsRunLocalPublishSerilogDirectory => RootDirectory / "Logs" / "RunLocalPublish" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsRunLocalPublishAuditDotNetDirectory => RootDirectory / "Logs" / "RunLocalPublish" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsRunLocalHotReloadSerilogDirectory => RootDirectory / "Logs" / "RunLocalHotReload" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsRunLocalHotReloadAuditDotNetDirectory => RootDirectory / "Logs" / "RunLocalHotReload" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestE2eSerilogDirectory => RootDirectory / "Logs" / "Test" / "e2e" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestE2eAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "e2e" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerSerilogDirectory => RootDirectory / "Logs" / "Test" / "Server" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Server" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerPostman => RootDirectory / "Logs" / "Test" / "Postman";
  #endregion

  internal Target PathsCleanDirectories => _ => _
    .Description("Pre-create directories that Docker containers need to prevent root permission issues")
    .Executes(() =>
    {
      // Create or clean Reports directories
      ReportsServerDirectory.CreateOrCleanDirectory();
      ReportsClientDirectory.CreateOrCleanDirectory();
      ReportsClientResultsDirectory.CreateOrCleanDirectory();
      ReportsClientArtifactsDirectory.CreateOrCleanDirectory();
      ReportsTestE2eDirectory.CreateOrCleanDirectory();
      ReportsTestE2eResultsDirectory.CreateOrCleanDirectory();
      ReportsTestE2eArtifactsDirectory.CreateOrCleanDirectory();

      // Create or clean Logs directories
      LogsDirectory.CreateOrCleanDirectory();
      LogsRunLocalHotReloadAuditDotNetDirectory.CreateOrCleanDirectory();
      LogsRunLocalHotReloadSerilogDirectory.CreateOrCleanDirectory();
      LogsRunLocalPublishAuditDotNetDirectory.CreateOrCleanDirectory();
      LogsRunLocalPublishSerilogDirectory.CreateOrCleanDirectory();
      LogsTestE2eAuditDotNetDirectory.CreateOrCleanDirectory();
      LogsTestE2eSerilogDirectory.CreateOrCleanDirectory();
      LogsTestServerSerilogDirectory.CreateOrCleanDirectory();
      LogsTestServerAuditDotNetDirectory.CreateOrCleanDirectory();

      var postmanCollections = new List<string>
      {
        "Auth",
        "ArticlesEmpty",
        "Article",
        "FeedAndArticles",
        "Profiles",
      };

      LogsTestServerPostman.CreateOrCleanDirectory();
      ReportsTestPostmanDirectory.CreateOrCleanDirectory();

      foreach (var collection in postmanCollections)
      {
        (LogsTestServerPostman / collection / "Server.Web" / "Serilog").CreateOrCleanDirectory();
        (LogsTestServerPostman / collection / "Server.Web" / "Audit.NET").CreateOrCleanDirectory();
        (ReportsTestPostmanDirectory / collection / "Artifacts").CreateOrCleanDirectory();
        (ReportsTestPostmanDirectory / collection / "Results").CreateOrCleanDirectory();
      }

      Log.Information("✓ Directories cleaned and pre-created");
    });
}
