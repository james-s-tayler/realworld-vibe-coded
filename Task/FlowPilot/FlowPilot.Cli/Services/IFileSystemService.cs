namespace FlowPilot.Cli.Services;

/// <summary>
/// Interface for file system operations to enable testing.
/// </summary>
public interface IFileSystemService
{
  bool FileExists(string path);

  bool DirectoryExists(string path);

  void CreateDirectory(string path);

  string ReadAllText(string path);

  void WriteAllText(string path, string content);

  void CopyFile(string sourceFile, string destFile);

  string GetCurrentDirectory();

  string[] GetFiles(string path, string searchPattern);

  string[] GetDirectories(string path);
}
