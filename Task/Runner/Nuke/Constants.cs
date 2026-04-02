using System.Text;
using Nuke.Common.IO;

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

  public static class Worktree
  {
    private const string WorktreeMarker = ".claude/worktrees/";

    public static string GetSlug(AbsolutePath rootDir)
    {
      return ((string)rootDir).Split('/').Last().ToLowerInvariant();
    }

    public static bool IsMainCheckout(AbsolutePath rootDir)
    {
      return !((string)rootDir).Contains(WorktreeMarker);
    }

    public static int GetPortOffset(AbsolutePath rootDir)
    {
      if (IsMainCheckout(rootDir))
      {
        return 0;
      }

      var slug = GetSlug(rootDir);
      var hash = Fnv1A(slug);

      // Map to range [100, 9900] in steps of 100
      var bucket = (int)(hash % 99) + 1;
      return bucket * 100;
    }

    public static string ScopedName(AbsolutePath rootDir, string baseName)
    {
      if (IsMainCheckout(rootDir))
      {
        return baseName;
      }

      return $"{GetSlug(rootDir)}-{baseName}";
    }

    private static uint Fnv1A(string input)
    {
      const uint fnvOffsetBasis = 2166136261;
      const uint fnvPrime = 16777619;

      var hash = fnvOffsetBasis;
      foreach (var b in Encoding.UTF8.GetBytes(input))
      {
        hash ^= b;
        hash *= fnvPrime;
      }

      return hash;
    }
  }
}
