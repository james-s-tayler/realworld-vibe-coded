namespace E2eTests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestCoverageAttribute : Attribute
{
  public required string Id { get; init; }

  public required string FeatureArea { get; init; }

  public required string Behavior { get; init; }

  public string[] Verifies { get; init; } = [];
}
