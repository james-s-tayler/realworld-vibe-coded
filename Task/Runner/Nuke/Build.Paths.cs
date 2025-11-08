using Nuke.Common.IO;

public partial class Build
{
  // Paths

  #region App/Server

  internal AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";

  internal AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";

  internal AbsolutePath ServerInfrastructureProject => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Server.Infrastructure.csproj";

  internal AbsolutePath MigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations";

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
  #endregion

  #region Logs
  internal AbsolutePath LogsDirectory => RootDirectory / "Logs";

  internal AbsolutePath LogsServerWebDirectory => RootDirectory / "Logs" / "Server.Web";

  internal AbsolutePath LogsSerilogDirectory => RootDirectory / "Logs" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsAuditDotNetDirectory => RootDirectory / "Logs" / "Server.Web" / "Audit.NET";
  #endregion
}
