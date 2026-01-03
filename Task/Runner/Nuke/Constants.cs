namespace Nuke;

public static class Constants
{
  public static class Docker
  {
    public static class Projects
    {
      public const string App = "app";
      public const string DevDependencies = "dev-dependencies";
    }

    public static class Networks
    {
      public const string AppNetwork = "app-network";
    }

    public static class Volumes
    {
      public const string SqlServer = $"{Projects.DevDependencies}_sqlserver-data";
    }
  }
}
