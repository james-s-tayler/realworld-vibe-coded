namespace FlowPilot.Cli.Services;

/// <summary>
/// Default implementation of file system operations.
/// </summary>
public class FileSystemService : IFileSystemService
{
  public bool FileExists(string path) => File.Exists(path);

  public bool DirectoryExists(string path) => Directory.Exists(path);

  public void CreateDirectory(string path) => Directory.CreateDirectory(path);

  public string ReadAllText(string path) => File.ReadAllText(path);

  public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

  public void CopyFile(string sourceFile, string destFile) => File.Copy(sourceFile, destFile, true);

  public string GetCurrentDirectory() => Directory.GetCurrentDirectory();

  public string[] GetFiles(string path, string searchPattern) => Directory.GetFiles(path, searchPattern);
}
