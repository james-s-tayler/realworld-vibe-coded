using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class NukeTargetLocationAnalyzer : DiagnosticAnalyzer
{
  public static readonly DiagnosticDescriptor InvalidLocationRule = new DiagnosticDescriptor(
      "NUKE003",
      "Nuke target must be in an existing Build*.cs file",
      "Target '{0}' cannot be added to '{1}'. Nuke targets must only be added to existing Build*.cs files.",
      "Location",
      DiagnosticSeverity.Error,
      isEnabledByDefault: true,
      description: "New Nuke build targets should only be added to existing Build*.cs files in the Task/Runner/Nuke directory to maintain organization and prevent file sprawl.");

  // Hardcoded list of allowed files where Nuke targets can be defined
  // This list should match the actual Build*.cs files in the repository
  private static readonly ImmutableHashSet<string> AllowedFileNames = ImmutableHashSet.Create(
      "Build.cs",
      "Build.Build.cs",
      "Build.Db.cs",
      "Build.DbMigrations.cs",
      "Build.Install.cs",
      "Build.Lint.cs",
      "Build.Paths.cs",
      "Build.RunLocal.cs",
      "Build.Test.cs"
  );

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
      ImmutableArray.Create(InvalidLocationRule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeProperty, SyntaxKind.PropertyDeclaration);
  }

  private static void AnalyzeProperty(SyntaxNodeAnalysisContext context)
  {
    var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

    // Check if this is a Target property (has type "Target")
    if (!IsTargetProperty(propertyDeclaration))
      return;

    // Get the file name
    var filePath = propertyDeclaration.SyntaxTree.FilePath;
    var fileName = Path.GetFileName(filePath);

    // Check if the file is in the allowed list
    if (!AllowedFileNames.Contains(fileName))
    {
      var propertyName = propertyDeclaration.Identifier.ValueText;
      var diagnostic = Diagnostic.Create(
          InvalidLocationRule,
          propertyDeclaration.Identifier.GetLocation(),
          propertyName,
          fileName);

      context.ReportDiagnostic(diagnostic);
    }
  }

  private static bool IsTargetProperty(PropertyDeclarationSyntax property)
  {
    // Check if the property type is "Target"
    return property.Type is IdentifierNameSyntax identifierName &&
           identifierName.Identifier.ValueText == "Target";
  }
}
