using Nuke.Common.IO;

public partial class Build
{
  // Paths

  #region App/Server

  AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";

  AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";

  AbsolutePath ServerInfrastructureProject => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Server.Infrastructure.csproj";

  AbsolutePath MigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations";

  #endregion

  #region App/Client

  AbsolutePath ClientDirectory => RootDirectory / "App" / "Client";

  AbsolutePath ClientDistDirectory => ClientDirectory / "dist";

  #endregion

  #region Publish

  AbsolutePath PublishDirectory => RootDirectory / "publish";

  AbsolutePath PublishWwwRootDirectory => PublishDirectory / "wwwroot";

  #endregion

  #region Task

  AbsolutePath TaskRunnerDirectory => RootDirectory / "Task" / "Runner";

  AbsolutePath TaskLocalDevDirectory => RootDirectory / "Task" / "LocalDev";

  #endregion

  #region Reports
  AbsolutePath ReportsServerDirectory => RootDirectory / "Reports" / "Server";

  AbsolutePath ReportsServerResultsDirectory => RootDirectory / "Reports" / "Server" / "Results";

  AbsolutePath ReportsServerArtifactsDirectory => RootDirectory / "Reports" / "Server" / "Artifacts";

  AbsolutePath ReportsClientDirectory => RootDirectory / "Reports" / "Client";

  AbsolutePath ReportsClientResultsDirectory => RootDirectory / "Reports" / "Client" / "Results";

  AbsolutePath ReportsClientArtifactsDirectory => RootDirectory / "Reports" / "Client" / "Artifacts";

  AbsolutePath ReportsTestE2eDirectory => RootDirectory / "Reports" / "Test" / "e2e";

  AbsolutePath ReportsTestE2eResultsDirectory => RootDirectory / "Reports" / "Test" / "e2e" / "Results";

  AbsolutePath ReportsTestE2eArtifactsDirectory => RootDirectory / "Reports" / "Test" / "e2e" / "Artifacts";

  AbsolutePath ReportsTestPostmanDirectory => RootDirectory / "Reports" / "Test" / "Postman";
  #endregion

  #region Logs
  AbsolutePath LogsDirectory => RootDirectory / "Logs";

  AbsolutePath LogsServerWebDirectory => RootDirectory / "Logs" / "Server.Web";

  AbsolutePath LogsSerilogDirectory => RootDirectory / "Logs" / "Server.Web" / "Serilog";

  AbsolutePath LogsAuditDotNetDirectory => RootDirectory / "Logs" / "Server.Web" / "Audit.NET";
  #endregion
}
