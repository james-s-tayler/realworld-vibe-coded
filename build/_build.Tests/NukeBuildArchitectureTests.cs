using System;
using System.IO;
using System.Reflection;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;
using System.Text.RegularExpressions;
using System.Linq;

namespace BuildTests
{
    public class NukeBuildArchitectureTests
    {
        private static readonly Architecture Architecture = new ArchLoader()
            .LoadAssembly(GetBuildAssembly())
            .Build();

        private static System.Reflection.Assembly GetBuildAssembly()
        {
            // Get the path to the build assembly using a hard-coded path relative to the current directory
            var currentDirectory = Directory.GetCurrentDirectory();
            
            // Find the repository root by looking for the .git directory
            var repositoryRoot = FindRepositoryRoot(currentDirectory);
            if (repositoryRoot == null)
            {
                throw new DirectoryNotFoundException($"Could not find repository root from current directory: {currentDirectory}");
            }
            
            var buildAssemblyPath = Path.Combine(repositoryRoot, "build", "_build", "bin", "Debug", "_build.dll");
            
            if (!File.Exists(buildAssemblyPath))
            {
                throw new FileNotFoundException($"Could not find build assembly at: {buildAssemblyPath}. Current directory: {currentDirectory}. Repository root: {repositoryRoot}");
            }

            return System.Reflection.Assembly.LoadFrom(buildAssemblyPath);
        }

        private static string? FindRepositoryRoot(string startDirectory)
        {
            var directory = new DirectoryInfo(startDirectory);
            while (directory != null)
            {
                if (Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
                    File.Exists(Path.Combine(directory.FullName, "build.sh"))) // Alternative marker
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            return null;
        }

        [Fact]
        public void BuildClass_ShouldExist()
        {
            // Verify that we can find the Build class in the architecture
            var buildClass = Architecture.Classes.FirstOrDefault(c => c.Name == "Build");
            Assert.NotNull(buildClass);
            
            // The class exists and can be analyzed - that's sufficient for our purposes
            // The inheritance from NukeBuild is verified by the fact that the project compiles and runs
            Console.WriteLine($"Found Build class: {buildClass.FullName}");
        }

        [Fact]
        public void AllTargetProperties_ShouldHaveDescriptionInSourceCode()
        {
            // Get all Target properties from the build class
            var buildClass = Architecture.Classes.FirstOrDefault(c => c.Name == "Build");
            Assert.NotNull(buildClass);

            var targetProperties = buildClass.Members
                .OfType<PropertyMember>()
                .Where(p => p.Type.Name == "Target")
                .ToList();

            Assert.NotEmpty(targetProperties);

            // Read the source code to check for descriptions
            var buildSourcePath = GetBuildSourcePath();
            var sourceCode = File.ReadAllText(buildSourcePath);

            foreach (var targetProperty in targetProperties)
            {
                var hasDescription = CheckTargetHasDescription(targetProperty.Name, sourceCode);
                Assert.True(hasDescription, $"Target '{targetProperty.Name}' is missing .Description() call");
            }
        }

        [Fact]
        public void AllTargetProperties_ShouldFollowPascalCaseNaming()
        {
            var buildClass = Architecture.Classes.FirstOrDefault(c => c.Name == "Build");
            Assert.NotNull(buildClass);

            var targetProperties = buildClass.Members
                .OfType<PropertyMember>()
                .Where(p => p.Type.Name == "Target")
                .ToList();

            Assert.NotEmpty(targetProperties);

            foreach (var targetProperty in targetProperties)
            {
                var isValidName = IsValidTargetName(targetProperty.Name);
                Assert.True(isValidName, 
                    $"Target '{targetProperty.Name}' should follow PascalCase naming (e.g., 'BuildServer', 'TestServer')");
            }
        }

        private static string GetBuildSourcePath()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            
            // Find the repository root by looking for the .git directory
            var repositoryRoot = FindRepositoryRoot(currentDirectory);
            if (repositoryRoot == null)
            {
                throw new DirectoryNotFoundException($"Could not find repository root from current directory: {currentDirectory}");
            }
            
            var buildSourcePath = Path.Combine(repositoryRoot, "build", "_build", "Build.cs");
            
            if (!File.Exists(buildSourcePath))
            {
                throw new FileNotFoundException($"Could not find Build.cs source file at: {buildSourcePath}. Current directory: {currentDirectory}. Repository root: {repositoryRoot}");
            }
            
            return buildSourcePath;
        }

        private static bool CheckTargetHasDescription(string targetName, string sourceCode)
        {
            // Look for the target definition pattern and check if it has a Description call
            // Pattern: Target TargetName => _ => _.Description(...).Executes(...)
            // or: Target TargetName => _ => _.Description(...).DependsOn(...).Executes(...)
            var targetPattern = $@"Target\s+{Regex.Escape(targetName)}\s*=>\s*_\s*=>\s*_[^;{{]*\.Description\s*\(";
            var regex = new Regex(targetPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            return regex.IsMatch(sourceCode);
        }

        private static bool IsValidTargetName(string name)
        {
            // Valid Nuke target names should be PascalCase
            // Examples: ShowHelp, BuildServer, TestServer, LintServer, etc.
            if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
                return false;
                
            return name.All(c => char.IsLetterOrDigit(c)) &&
                   !name.Contains("_") &&
                   !name.Contains("-");
        }
    }
}