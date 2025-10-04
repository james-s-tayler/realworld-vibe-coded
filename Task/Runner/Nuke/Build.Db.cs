using Nuke.Common;

public partial class Build
{
  [Parameter("Force operation without confirmation")]
  readonly bool Force = false;

  Target DbReset => _ => _
    .Description("Delete local sqlite database (confirm or --force to skip)")
    .Executes(() =>
    {
      if (!Force)
      {
        Console.Write("Are you sure? [y/N] ");
        var response = Console.ReadLine();
        if (response?.ToLowerInvariant() != "y")
        {
          Console.WriteLine("Operation cancelled");
          return;
        }
      }

      Console.WriteLine($"Deleting {DatabaseFile}...");
      if (File.Exists(DatabaseFile))
      {
        File.Delete(DatabaseFile);
      }
      Console.WriteLine("Done.");
    });

  Target DbResetForce => _ => _
    .Description("Delete local sqlite database (no confirmation)")
    .Executes(() =>
    {
      Console.WriteLine($"Deleting {DatabaseFile}...");
      if (File.Exists(DatabaseFile))
      {
        File.Delete(DatabaseFile);
      }
      Console.WriteLine("Done.");
    });
}
