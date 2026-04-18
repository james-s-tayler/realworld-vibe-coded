using System.Reflection;
using System.Text.RegularExpressions;
using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using ArchUnitNET.xUnitV3;
using Nuke.Common;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace BuildTests
{
  public class NukeBuildArchitectureTests
  {
    private static readonly Architecture Architecture = new ArchLoader()
        .LoadAssembly(Assembly.GetAssembly(typeof(Build)))
        .Build();

    private static readonly IObjectProvider<Class> BuildClasses =
        Classes().That().AreAssignableTo(typeof(NukeBuild)).As("Build Classes");

    private static readonly IObjectProvider<PropertyMember> NukeTargets =
        PropertyMembers()
            .That().AreDeclaredIn(BuildClasses)
            .And().DependOnAny(typeof(Target));

    [Fact]
    public void BuildClassExists()
    {
      IArchRule buildClassesShouldNotBeEmpty = Classes().That().Are(BuildClasses).Should().Exist();
      buildClassesShouldNotBeEmpty.Check(Architecture);
    }

    [Fact]
    public void NukeTargetsExist()
    {
      PropertyMembers().That().Are(NukeTargets).Should().Exist().Check(Architecture);
    }

    [Fact]
    public void NukeTargetsShouldFollowNamingConventions()
    {
      IArchRule nukeTargetsShouldFollowNamingConventions =
          PropertyMembers().That().Are(NukeTargets)
          .Should().HaveNameStartingWith("Lint")
          .OrShould().HaveNameStartingWith("Build")
          .OrShould().HaveNameStartingWith("Test")
          .OrShould().HaveNameStartingWith("RunLocal")
          .OrShould().HaveNameStartingWith("Db")
          .OrShould().HaveNameStartingWith("Install")
          .OrShould().HaveNameStartingWith("Paths")
          .OrShould().HaveNameStartingWith("Archon")
          .Because("this is the established naming convention for Nuke build targets");

      nukeTargetsShouldFollowNamingConventions.Check(Architecture);
    }

    [Fact]
    public void LintTargetsShouldFollowLintNamingConvention()
    {
      var lintTargets = PropertyMembers()
          .That().Are(NukeTargets)
          .And().HaveNameStartingWith("Lint");

      IArchRule lintTargetsShouldEndWithVerifyOrFix = lintTargets
          .Should().HaveNameEndingWith("Verify")
          .OrShould().HaveNameEndingWith("Fix")
          .Because("Lint targets must end with either 'Verify' (for checking) or 'Fix' (for auto-fixing) to clarify their behavior");

      lintTargetsShouldEndWithVerifyOrFix.Check(Architecture);
    }

    [Fact]
    public void LintAllVerifyShouldDependOnAllLintVerifyTargets()
    {
      var lintVerifyTargets = typeof(Build)
          .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
          .Where(p => p.PropertyType == typeof(Target))
          .Where(p => p.Name.StartsWith("Lint") && p.Name.EndsWith("Verify"))
          .Where(p => p.Name != "LintAllVerify")
          .Select(p => p.Name)
          .ToList();

      Xunit.Assert.NotEmpty(lintVerifyTargets);

      var buildLintSource = FindSourceFile("Build.Lint.cs");
      var source = File.ReadAllText(buildLintSource);

      var match = Regex.Match(source, @"LintAllVerify.*?\.DependsOn\(([^)]+)\)", RegexOptions.Singleline);
      Xunit.Assert.True(match.Success, "LintAllVerify must have a .DependsOn() declaration in Build.Lint.cs");

      var dependsOnContent = match.Groups[1].Value;

      foreach (var target in lintVerifyTargets)
      {
        Xunit.Assert.True(
            dependsOnContent.Contains(target),
            $"LintAllVerify is missing dependency on {target}. Add it to .DependsOn() in Build.Lint.cs.");
      }
    }

    private static string FindSourceFile(string fileName)
    {
      var dir = new DirectoryInfo(AppContext.BaseDirectory);
      while (dir != null)
      {
        var candidate = Path.Combine(dir.FullName, "Nuke", fileName);
        if (File.Exists(candidate))
        {
          return candidate;
        }

        candidate = Path.Combine(dir.FullName, fileName);
        if (File.Exists(candidate))
        {
          return candidate;
        }

        dir = dir.Parent;
      }

      throw new FileNotFoundException($"Could not find {fileName} by walking up from {AppContext.BaseDirectory}");
    }
  }
}
