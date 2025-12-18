using Nuke.Common.IO;

public partial class Build
{
  // Paths

  #region App/Server

  internal AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";

  internal AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";

  internal AbsolutePath ServerInfrastructureProject => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Server.Infrastructure.csproj";

  internal AbsolutePath MigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations";

  internal AbsolutePath IdempotentScriptPath => MigrationsDirectory / "idempotent.sql";

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

  internal AbsolutePath LogsTestServerPostmanAuthSerilogDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "Auth" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerPostmanAuthAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "Auth" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerPostmanArticlesEmptySerilogDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "ArticlesEmpty" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerPostmanArticlesEmptyAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "ArticlesEmpty" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerPostmanArticleSerilogDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "Article" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerPostmanArticleAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "Article" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerPostmanFeedAndArticlesSerilogDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "FeedAndArticles" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerPostmanFeedAndArticlesAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "FeedAndArticles" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerPostmanProfilesSerilogDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "Profiles" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerPostmanProfilesAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Postman" / "Profiles" / "Server.Web" / "Audit.NET";
  #endregion
}
