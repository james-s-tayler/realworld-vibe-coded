# Nuke Analyzers Documentation

This directory contains custom Roslyn analyzers for the Nuke build system to enforce best practices and maintain consistency.

## Analyzers

### NUKE001: Nuke target is missing description
**File:** `NukeTargetDescriptionAnalyzer.cs`

**Purpose:** Ensures all Nuke build targets include a `.Description()` call to document their purpose.

**Severity:** Error

**Example Violation:**
```csharp
Target MyTarget => _ => _
    .Executes(() => { }); // Missing .Description()
```

**Fix:**
```csharp
Target MyTarget => _ => _
    .Description("Describe what this target does")
    .Executes(() => { });
```

---

### NUKE002: Direct System.IO usage is not allowed
**File:** `BanSystemIOAnalyzer.cs`

**Purpose:** Prevents direct usage of `System.IO.File`, `System.IO.Directory`, and `System.IO.Path` classes to enforce the use of Nuke's `AbsolutePath` API for better cross-platform support and consistency.

**Severity:** Error

**Rationale:** 
- `AbsolutePath` provides a unified, type-safe API for file system operations
- Avoids relative path issues and platform-specific path separator problems
- Integrates seamlessly with Nuke's build system

**Common Violations and Fixes:**

#### File Operations
```csharp
// ❌ Violation
if (File.Exists(path))
{
    var content = File.ReadAllText(path);
    File.WriteAllLines(path, lines);
}

// ✅ Correct
AbsolutePath file = RootDirectory / "path" / "to" / "file.txt";
if (file.FileExists())
{
    var content = file.ReadAllText();
    file.WriteAllLines(lines);
}
```

#### Directory Operations
```csharp
// ❌ Violation
if (Directory.Exists(dirPath))
{
    Directory.Delete(dirPath, true);
}
Directory.CreateDirectory(dirPath);

// ✅ Correct
AbsolutePath dir = RootDirectory / "path" / "to" / "dir";
if (dir.DirectoryExists())
{
    dir.DeleteDirectory();
}
dir.CreateDirectory();

// Or use the convenience method
dir.CreateOrCleanDirectory();
```

#### Path Operations
```csharp
// ❌ Violation
var combined = Path.Combine(baseDir, "subdir", "file.txt");
var relative = Path.GetRelativePath(baseDir, filePath);
var fileName = Path.GetFileName(filePath);

// ✅ Correct
var combined = baseDir / "subdir" / "file.txt";
var relative = baseDir.GetRelativePathTo(filePath);
var fileName = filePath.Name;
```

#### Globbing Files and Directories
```csharp
// ❌ Violation
var files = Directory.GetFiles(dir, "*.txt", SearchOption.AllDirectories);
var subdirs = Directory.GetDirectories(dir);

// ✅ Correct
var files = dir.GlobFiles("**/*.txt");
var subdirs = dir.GlobDirectories("*");
```

#### File/Directory Info
```csharp
// ❌ Violation
var fileInfo = new FileInfo(path);
var lastWrite = File.GetLastWriteTime(path);

// ✅ Correct
AbsolutePath file = RootDirectory / "path" / "to" / "file.txt";
var fileInfo = file.ToFileInfo();
var lastWrite = fileInfo.LastWriteTime;
```

**Common AbsolutePath Methods:**
- `.FileExists()` / `.DirectoryExists()` / `.Exists()`
- `.ReadAllText()` / `.ReadAllLines()` / `.ReadAllBytes()`
- `.WriteAllText()` / `.WriteAllLines()` / `.WriteAllBytes()`
- `.TouchFile()` - Create file if doesn't exist, update timestamp if it does
- `.CreateDirectory()` / `.DeleteDirectory()`
- `.CreateOrCleanDirectory()` - Create or delete and recreate directory
- `.Copy(destination, ExistsPolicy)` / `.Move(destination)`
- `.ToFileInfo()` / `.ToDirectoryInfo()` - Get System.IO FileInfo/DirectoryInfo
- `.GlobFiles(pattern)` / `.GlobDirectories(pattern)` - Find files/dirs matching pattern
- `.Name` / `.NameWithoutExtension` / `.Extension` / `.Parent` - Path properties
- `.GetRelativePathTo(target)` - Get relative path from this to target

---

### NUKE003: Nuke target must be in an existing Build*.cs file
**File:** `NukeTargetLocationAnalyzer.cs`

**Purpose:** Ensures new Nuke build targets are only added to existing `Build*.cs` files to maintain organization and prevent file sprawl.

**Severity:** Error

**Allowed Files:**
- `Build.cs`
- `Build.Build.cs`
- `Build.Db.cs`
- `Build.DbMigrations.cs`
- `Build.Install.cs`
- `Build.Lint.cs`
- `Build.Paths.cs`
- `Build.RunLocal.cs`
- `Build.Test.cs`

**Rationale:**
- Maintains a consistent file structure
- Prevents accidental creation of new build files
- Encourages developers to group related targets in existing files

**Limitation:**
This analyzer uses a hardcoded allowlist of file names because Roslyn analyzers do not have access to VCS (Version Control System) information. This means:
- The analyzer cannot detect which files already exist in the repository
- The allowlist must be manually updated if new legitimate Build files are added
- The analyzer is designed to prevent new files from being created, not to dynamically adapt to existing files

**If You Need to Add a New Build File:**
If you legitimately need to add a new `Build.*.cs` file for organizational purposes, you must:
1. Add the new file name to the `AllowedFileNames` list in `NukeTargetLocationAnalyzer.cs`
2. Document the purpose of the new file in this README
3. Get approval through code review

**Example Violation:**
```csharp
// File: Build.MyNewFeature.cs
partial class Build
{
    Target MyTarget => _ => _
        .Description("A target in a new file")
        .Executes(() => { });
}
// Error: Target 'MyTarget' cannot be added to 'Build.MyNewFeature.cs'
```

**Fix:**
Add the target to one of the existing Build files, choosing the most appropriate one based on the target's purpose:
- `Build.Build.cs` - Building and publishing targets
- `Build.Test.cs` - Testing targets
- `Build.Lint.cs` - Linting and formatting targets
- `Build.Install.cs` - Installation and setup targets
- `Build.RunLocal.cs` - Local development targets
- `Build.Db.cs` / `Build.DbMigrations.cs` - Database-related targets
- `Build.Paths.cs` - Path definitions (not typically for targets)

---

## Building and Testing

The analyzers are automatically applied when building the Nuke project:

```bash
dotnet build Task/Runner/Nuke.sln
```

To run the Nuke linter which includes these analyzers:

```bash
./build.sh LintNukeVerify
```

To run tests:

```bash
dotnet test Task/Runner/Nuke.Tests/Nuke.Tests.csproj
```

## Technical Details

- **Target Framework:** netstandard2.0 (required for Roslyn analyzers)
- **Language Version:** C# 7.3 (limited by netstandard2.0)
- **Analyzer Registration:** Analyzers are referenced as project analyzers in `Nuke.csproj`
- **Diagnostic Severity:** All diagnostics are set to `Error` level to ensure they block builds

## Suppressing Analyzer Warnings

In rare cases where you need to use System.IO directly (e.g., for functionality not available in AbsolutePath), you can suppress the warning with a justification:

```csharp
// NUKE002: Using raw System.IO is necessary here because [explain why]
#pragma warning disable NUKE002
var info = new FileInfo(path);
#pragma warning restore NUKE002
```

**Important:** Suppressions should be rare and must include a clear justification comment explaining why the AbsolutePath API cannot be used.
