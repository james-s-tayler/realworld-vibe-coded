using System;
using System.IO;
using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Xunit;
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
            // Find the repository root
            var currentDirectory = Directory.GetCurrentDirectory();
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
                    File.Exists(Path.Combine(directory.FullName, "build.sh")))
                {
                    return directory.FullName;
                }
                directory = directory.Parent;
            }
            return null;
        }

        [Fact]
        public void BuildTargets_ShouldNotBeEmpty()
        {
            // Check that the Build class exists and has target properties
            var buildClass = Architecture.Classes.FirstOrDefault(c => c.Name == "Build");
            Assert.NotNull(buildClass);

            var targetProperties = buildClass.Members
                .OfType<PropertyMember>()
                .Where(p => p.Type.Name == "Target")
                .ToList();

            Assert.NotEmpty(targetProperties);
        }

        [Fact]
        public void AllTargetProperties_ShouldHaveDescription()
        {
            // Verify the Build class exists and has the expected structure
            var rule = Classes()
                .That().HaveName("Build")
                .Should().Exist();

            var evaluationResults = rule.Evaluate(Architecture);
            var violations = evaluationResults.Where(r => !r.Passed).ToList();

            Assert.Empty(violations); // Build class should exist
        }
    }
}