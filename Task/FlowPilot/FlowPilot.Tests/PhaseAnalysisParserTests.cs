using FlowPilot.Cli.Models;
using FlowPilot.Cli.Services;
using Xunit;

namespace FlowPilot.Tests;

public class PhaseAnalysisParserTests
{
  [Fact]
  public void ParsePhases_WithPrBoundary_ReturnsPhasesWithBoundaryMarked()
  {
    // Arrange
    var fileSystem = new FakeFileSystemService();
    var parser = new PhaseAnalysisParser(fileSystem);

    var content = @"## Phase Analysis

### phase_1

**Goal**: First phase

**PR Boundary**: yes

---

### phase_2

**Goal**: Second phase

**PR Boundary**: no

---

### phase_3

**Goal**: Third phase

**PR Boundary**: yes

---";

    fileSystem.WriteAllText("/test/phase-analysis.md", content);

    // Act
    var phases = parser.ParsePhases("/test/phase-analysis.md");

    // Assert
    Assert.Equal(3, phases.Count);

    Assert.Equal(1, phases[0].PhaseNumber);
    Assert.Equal("phase_1", phases[0].PhaseName);
    Assert.True(phases[0].IsPullRequestBoundary);

    Assert.Equal(2, phases[1].PhaseNumber);
    Assert.Equal("phase_2", phases[1].PhaseName);
    Assert.False(phases[1].IsPullRequestBoundary);

    Assert.Equal(3, phases[2].PhaseNumber);
    Assert.Equal("phase_3", phases[2].PhaseName);
    Assert.True(phases[2].IsPullRequestBoundary);
  }

  [Fact]
  public void ParsePhases_WithoutPrBoundary_DefaultsToFalse()
  {
    // Arrange
    var fileSystem = new FakeFileSystemService();
    var parser = new PhaseAnalysisParser(fileSystem);

    var content = @"## Phase Analysis

### phase_1

**Goal**: First phase

---

### phase_2

**Goal**: Second phase

---";

    fileSystem.WriteAllText("/test/phase-analysis.md", content);

    // Act
    var phases = parser.ParsePhases("/test/phase-analysis.md");

    // Assert
    Assert.Equal(2, phases.Count);
    Assert.False(phases[0].IsPullRequestBoundary);
    Assert.False(phases[1].IsPullRequestBoundary);
  }

  [Fact]
  public void ParsePhases_FileDoesNotExist_ReturnsEmptyList()
  {
    // Arrange
    var fileSystem = new FakeFileSystemService();
    var parser = new PhaseAnalysisParser(fileSystem);

    // Act
    var phases = parser.ParsePhases("/nonexistent/phase-analysis.md");

    // Assert
    Assert.Empty(phases);
  }

  private class FakeFileSystemService : IFileSystemService
  {
    private readonly Dictionary<string, string> _files = new();

    public bool FileExists(string path) => _files.ContainsKey(path);

    public string ReadAllText(string path) => _files.TryGetValue(path, out var content) ? content : throw new FileNotFoundException();

    public void WriteAllText(string path, string content) => _files[path] = content;

    public void CopyFile(string sourcePath, string destinationPath) => throw new NotImplementedException();

    public string[] GetFiles(string path, string searchPattern) => throw new NotImplementedException();

    public bool DirectoryExists(string path) => throw new NotImplementedException();

    public void CreateDirectory(string path) => throw new NotImplementedException();

    public string[] GetDirectories(string path) => throw new NotImplementedException();

    public string GetCurrentDirectory() => throw new NotImplementedException();
  }
}
