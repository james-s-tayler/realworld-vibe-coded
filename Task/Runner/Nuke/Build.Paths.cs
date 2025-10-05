using Nuke.Common.IO;

public partial class Build
{
  // Paths

  #region App/Server

  AbsolutePath ServerSolution => RootDirectory / "App" / "Server" / "Server.sln";
  AbsolutePath ServerProject => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "Server.Web.csproj";
  AbsolutePath DatabaseFile => RootDirectory / "App" / "Server" / "src" / "Server.Web" / "database.sqlite";

  #endregion

  #region App/Client

  AbsolutePath ClientDirectory => RootDirectory / "App" / "Client";

  #endregion

  #region Task

  AbsolutePath TaskRunnerDirectory => RootDirectory / "Task" / "Runner";

  #endregion

  #region Reports
  AbsolutePath ReportsServerDirectory => RootDirectory / "Reports" / "Server";
  AbsolutePath ReportsServerResultsDirectory => RootDirectory / "Reports" / "Server" / "Results";
  AbsolutePath ReportsServerArtifactsDirectory => RootDirectory / "Reports" / "Server" / "Artifacts";
  AbsolutePath ReportsTestE2eDirectory => RootDirectory / "Reports" / "Test" / "e2e";
  AbsolutePath ReportsTestE2eResultsDirectory => RootDirectory / "Reports" / "Test" / "e2e" / "Results";
  AbsolutePath ReportsTestE2eArtifactsDirectory => RootDirectory / "Reports" / "Test" / "e2e" / "Artifacts";
  AbsolutePath ReportsTestPostmanDirectory => RootDirectory / "Reports" / "Test" / "Postman";
  #endregion
}
