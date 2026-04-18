using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Bans .OverridePropertyName() inside FastEndpoints Validator subclasses.
  /// The global ValidatorOptions.Global.PropertyNameResolver collapses property chains to the leaf
  /// member name, which combined with FastEndpoints' camelCase policy produces the spec-compliant
  /// field name automatically. Per-rule overrides are therefore redundant.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanOverridePropertyNameInValidatorAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV022";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Do not call .OverridePropertyName() in FastEndpoints Validator",
        ".OverridePropertyName() is redundant: the global PropertyNameResolver strips chains to the leaf member name and FastEndpoints' camelCase policy produces the final field name. Remove this call.",
        "FastEndpoints",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Global ValidatorOptions.Global.PropertyNameResolver already returns the leaf member name. Per-rule .OverridePropertyName() calls duplicate that behavior and drift easily when the underlying property is renamed.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
      var invocation = (InvocationExpressionSyntax)context.Node;

      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess ||
          memberAccess.Name.Identifier.Text != "OverridePropertyName")
      {
        return;
      }

      if (!IsInValidatorClass(context))
      {
        return;
      }

      var diagnostic = Diagnostic.Create(Rule, memberAccess.Name.GetLocation());
      context.ReportDiagnostic(diagnostic);
    }

    private static bool IsInValidatorClass(SyntaxNodeAnalysisContext context)
    {
      var node = context.Node;
      while (node != null)
      {
        if (node is ClassDeclarationSyntax classDecl)
        {
          var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
          if (classSymbol != null)
          {
            return InheritsFromValidator(classSymbol);
          }
        }

        node = node.Parent;
      }

      return false;
    }

    private static bool InheritsFromValidator(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        if (baseType.Name == "Validator" &&
            baseType.ContainingNamespace?.ToDisplayString() == "FastEndpoints")
        {
          return true;
        }

        baseType = baseType.BaseType;
      }

      return false;
    }
  }
}
