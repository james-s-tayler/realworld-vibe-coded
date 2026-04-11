using Nuke;
using Serilog;

public partial class Build
{
  internal Dictionary<string, string> GetWorktreeEnvVars()
  {
    var offset = Constants.Worktree.GetPortOffset(RootDirectory);
    return new Dictionary<string, string>
    {
      ["PORT_API_HTTP"] = (5000 + offset).ToString(),
      ["PORT_API_HTTPS"] = (5001 + offset).ToString(),
      ["PORT_SQL"] = (1433 + offset).ToString(),
      ["PORT_SEQ"] = (5341 + offset).ToString(),
      ["PORT_GELF"] = (12201 + offset).ToString(),
      ["PORT_OTLP"] = (4317 + offset).ToString(),
      ["PORT_JAEGER_UI"] = (16686 + offset).ToString(),
      ["PORT_PROMETHEUS"] = (9090 + offset).ToString(),
      ["PORT_GRAFANA"] = (3000 + offset).ToString(),
      ["NETWORK_NAME"] = ScopedNetworkName,
      ["IMAGE_TAG"] = Constants.Worktree.IsMainCheckout(RootDirectory) ? "local" : Constants.Worktree.GetSlug(RootDirectory),
      ["COOKIE_SUFFIX"] = Constants.Worktree.IsMainCheckout(RootDirectory) ? string.Empty : $".{Constants.Worktree.GetSlug(RootDirectory)}",
    };
  }

  internal string ScopedProjectName(string baseName)
  {
    return Constants.Worktree.ScopedName(RootDirectory, baseName);
  }

  internal string ScopedNetworkName => Constants.Worktree.ScopedName(RootDirectory, Constants.Docker.Networks.AppNetwork);

  internal string SqlServerVolumeName => $"{ScopedProjectName(Constants.Docker.Projects.DevDependencies)}_sqlserver-data";

  internal int DocsMcpPort => 6280 + Constants.Worktree.GetPortOffset(RootDirectory);

  internal void LogWorktreeInfo()
  {
    var offset = Constants.Worktree.GetPortOffset(RootDirectory);
    var slug = Constants.Worktree.GetSlug(RootDirectory);
    var isMain = Constants.Worktree.IsMainCheckout(RootDirectory);

    Log.Information("Worktree Info:");
    Log.Information("  Root:       {Root}", RootDirectory);
    Log.Information("  Slug:       {Slug}", slug);
    Log.Information("  Main:       {IsMain}", isMain);
    Log.Information("  Offset:     {Offset}", offset);
    Log.Information("  Network:    {Network}", ScopedNetworkName);
    Log.Information("  Image Tag:  {Tag}", isMain ? "local" : slug);
    Log.Information("  Volume:     {Volume}", SqlServerVolumeName);
    Log.Information("Port Mappings:");
    Log.Information("  API HTTP:   {Port}", 5000 + offset);
    Log.Information("  API HTTPS:  {Port}", 5001 + offset);
    Log.Information("  SQL Server: {Port}", 1433 + offset);
    Log.Information("  Seq:        {Port}", 5341 + offset);
    Log.Information("  GELF:       {Port}", 12201 + offset);
    Log.Information("  OTLP:       {Port}", 4317 + offset);
    Log.Information("  Jaeger UI:  {Port}", 16686 + offset);
    Log.Information("  Prometheus: {Port}", 9090 + offset);
    Log.Information("  Grafana:    {Port}", 3000 + offset);
    Log.Information("  Vite:       {Port}", 5173 + offset);
    Log.Information("  Docs MCP:   {Port}", DocsMcpPort);
  }
}
