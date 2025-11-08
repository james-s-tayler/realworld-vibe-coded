using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Analyzer that forbids the use of EndpointWithoutRequest in endpoint definitions.
  /// Endpoints should always have explicit request types for better type safety and clarity.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EndpointWithoutRequestAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV005";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Do not use EndpointWithoutRequest",
        "Do not use EndpointWithoutRequest. Use Endpoint<TRequest, TResponse> instead to ensure explicit request types and better type safety.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "EndpointWithoutRequest should be avoided. All endpoints should have explicit request types using Endpoint<TRequest, TResponse> or similar generic endpoint base classes for better type safety, clarity, and consistency.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
      var classDecl = (ClassDeclarationSyntax)context.Node;

      // Check if the class has a base type
      if (classDecl.BaseList == null || classDecl.BaseList.Types.Count == 0)
      {
        return;
      }

      // Get the semantic model to resolve type information
      var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
      if (classSymbol == null)
      {
        return;
      }

      // Check if the class inherits from EndpointWithoutRequest
      if (InheritsFromEndpointWithoutRequest(classSymbol))
      {
        // Find the base type syntax in the base list
        foreach (var baseType in classDecl.BaseList.Types)
        {
          var typeInfo = context.SemanticModel.GetTypeInfo(baseType.Type);
          if (typeInfo.Type != null && IsEndpointWithoutRequest(typeInfo.Type))
          {
            var diagnostic = Diagnostic.Create(Rule, baseType.GetLocation());
            context.ReportDiagnostic(diagnostic);
            break;
          }
        }
      }
    }

    private static bool InheritsFromEndpointWithoutRequest(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        if (IsEndpointWithoutRequest(baseType))
        {
          return true;
        }
        baseType = baseType.BaseType;
      }
      return false;
    }

    private static bool IsEndpointWithoutRequest(ITypeSymbol typeSymbol)
    {
      // Check if it's EndpointWithoutRequest from FastEndpoints
      if (typeSymbol.Name == "EndpointWithoutRequest" &&
          typeSymbol.ContainingNamespace?.ToDisplayString() == "FastEndpoints")
      {
        return true;
      }

      return false;
    }
  }
}
