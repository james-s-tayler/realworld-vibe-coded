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
  }
}
