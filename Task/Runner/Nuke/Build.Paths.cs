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

  internal AbsolutePath AppSettingsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Web";

  internal AbsolutePath SchemaDirectory => RootDirectory / "App" / "Server" / "schemas";

  internal AbsolutePath MigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "Migrations";

  internal AbsolutePath IdempotentScriptPath => MigrationsDirectory / "idempotent.sql";

  internal AbsolutePath TenantStoreMigrationsDirectory => RootDirectory / "App" / "Server" / "src" / "Server.Infrastructure" / "Data" / "TenantStoreMigrations";

  internal AbsolutePath TenantStoreIdempotentScriptPath => TenantStoreMigrationsDirectory / "idempotent.sql";

  #endregion

  #region App/Client

  internal AbsolutePath ClientDirectory => RootDirectory / "App" / "Client";

  internal AbsolutePath ClientDistDirectory => ClientDirectory / "dist";

  internal AbsolutePath ClientApiGeneratedDirectory => ClientDirectory / "src" / "api" / "generated";

  #endregion

  #region Publish

  internal AbsolutePath PublishDirectory => RootDirectory / "publish";

  internal AbsolutePath PublishWwwRootDirectory => PublishDirectory / "wwwroot";

  #endregion

  #region Task

  internal AbsolutePath TaskRunnerDirectory => RootDirectory / "Task" / "Runner";

  internal AbsolutePath TaskLocalDevDirectory => RootDirectory / "Task" / "LocalDev";

  internal AbsolutePath DockerComposeDependencies => TaskLocalDevDirectory / "docker-compose.dev-deps.yml";

  internal AbsolutePath RoslynMcpProject => RootDirectory / "Task" / "McpServers" / "roslyn-mcp" / "RoslynMCP" / "RoslynMCP.csproj";

  internal AbsolutePath LocalNuGetFeedDirectory => RootDirectory / ".local-nuget";

  #endregion

  #region Claude

  internal AbsolutePath ClaudeMdFile => RootDirectory / "CLAUDE.md";

  internal AbsolutePath ClaudeRulesDirectory => RootDirectory / ".claude" / "rules";

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

  #endregion

  #region Logs
  internal AbsolutePath LogsDirectory => RootDirectory / "Logs";

  internal AbsolutePath LogsRunLocalPublishSerilogDirectory => RootDirectory / "Logs" / "RunLocalPublish" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsRunLocalPublishAuditDotNetDirectory => RootDirectory / "Logs" / "RunLocalPublish" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestE2eSerilogDirectory => RootDirectory / "Logs" / "Test" / "e2e" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestE2eAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "e2e" / "Server.Web" / "Audit.NET";

  internal AbsolutePath LogsTestServerSerilogDirectory => RootDirectory / "Logs" / "Test" / "Server" / "Server.Web" / "Serilog";

  internal AbsolutePath LogsTestServerAuditDotNetDirectory => RootDirectory / "Logs" / "Test" / "Server" / "Server.Web" / "Audit.NET";

  #endregion

  internal Target PathsShowWorktreeInfo => _ => _
    .Description("Show worktree isolation info: slug, port offset, and all port mappings")
    .Executes(LogWorktreeInfo);

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
      LogsRunLocalPublishAuditDotNetDirectory.CreateOrCleanDirectory();
      LogsRunLocalPublishSerilogDirectory.CreateOrCleanDirectory();
      LogsTestE2eAuditDotNetDirectory.CreateOrCleanDirectory();
      LogsTestE2eSerilogDirectory.CreateOrCleanDirectory();
      LogsTestServerSerilogDirectory.CreateOrCleanDirectory();
      LogsTestServerAuditDotNetDirectory.CreateOrCleanDirectory();

      Log.Information("✓ Directories cleaned and pre-created");
    });
}
