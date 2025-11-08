using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Analyzer that enforces FastEndpoints grouping rules for Server.Web.DevOnly namespace:
  /// 1. All endpoints in Server.Web.DevOnly must call Group<DevOnly>() or Group<T>() where T : SubGroup<DevOnly>
  /// 2. Any endpoint calling Group<DevOnly>() or Group<T>() where T : SubGroup<DevOnly> must be in Server.Web.DevOnly namespace
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class DevOnlyGroupAnalyzer : DiagnosticAnalyzer
  {
    public const string MissingDevOnlyGroupDiagnosticId = "SRV012";
    public const string WrongNamespaceDiagnosticId = "SRV013";

    private static readonly DiagnosticDescriptor MissingDevOnlyGroupRule = new DiagnosticDescriptor(
        MissingDevOnlyGroupDiagnosticId,
        "DevOnly endpoint must call Group<DevOnly> or SubGroup",
        "Endpoint '{0}' in Server.Web.DevOnly namespace must call Group<DevOnly>() or Group<T>() where T inherits from SubGroup<DevOnly> in its Configure() method",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All FastEndpoints endpoints in the Server.Web.DevOnly namespace must be explicitly grouped under DevOnly or a SubGroup of DevOnly.");

    private static readonly DiagnosticDescriptor WrongNamespaceRule = new DiagnosticDescriptor(
        WrongNamespaceDiagnosticId,
        "DevOnly group used outside Server.Web.DevOnly namespace",
        "Endpoint '{0}' calls Group<{1}>() but is not in the Server.Web.DevOnly namespace. Only endpoints in Server.Web.DevOnly can use DevOnly groups.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Endpoints using Group<DevOnly> or Group<T> where T : SubGroup<DevOnly> must be located in the Server.Web.DevOnly namespace.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(MissingDevOnlyGroupRule, WrongNamespaceRule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
    }

    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
      var classDecl = (ClassDeclarationSyntax)context.Node;
      var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
      if (classSymbol == null)
      {
        return;
      }

      // Check if this is a FastEndpoints endpoint (inherits from Endpoint or similar)
      if (!IsEndpoint(classSymbol))
      {
        return;
      }

      var containingNamespace = classSymbol.ContainingNamespace?.ToDisplayString();
      bool isInDevOnlyNamespace = containingNamespace?.StartsWith("Server.Web.DevOnly") == true;

      // Find the Configure method
      var configureMethod = classDecl.Members
          .OfType<MethodDeclarationSyntax>()
          .FirstOrDefault(m => m.Identifier.Text == "Configure");

      if (configureMethod == null)
      {
        return;
      }

      // Find all Group<T>() invocations in the Configure method
      var groupInvocations = configureMethod.DescendantNodes()
          .OfType<InvocationExpressionSyntax>()
          .Where(inv => IsGroupInvocation(inv, context.SemanticModel))
          .ToList();

      // Check if any Group call uses DevOnly or SubGroup<DevOnly>
      bool hasDevOnlyGroup = false;
      GenericNameSyntax devOnlyGroupCall = null;

      foreach (var invocation in groupInvocations)
      {
        if (invocation.Expression is GenericNameSyntax genericName)
        {
          var typeArg = genericName.TypeArgumentList.Arguments.FirstOrDefault();
          if (typeArg != null)
          {
            var typeInfo = context.SemanticModel.GetTypeInfo(typeArg);
            if (typeInfo.Type != null && IsDevOnlyOrSubGroupOfDevOnly(typeInfo.Type))
            {
              hasDevOnlyGroup = true;
              devOnlyGroupCall = genericName;
              break;
            }
          }
        }
      }

      // Rule 1: If in Server.Web.DevOnly namespace, must have DevOnly group
      if (isInDevOnlyNamespace && !hasDevOnlyGroup)
      {
        var diagnostic = Diagnostic.Create(
            MissingDevOnlyGroupRule,
            classDecl.Identifier.GetLocation(),
            classSymbol.Name);
        context.ReportDiagnostic(diagnostic);
      }

      // Rule 2: If has DevOnly group, must be in Server.Web.DevOnly namespace
      if (hasDevOnlyGroup && !isInDevOnlyNamespace && devOnlyGroupCall != null)
      {
        var typeArg = devOnlyGroupCall.TypeArgumentList.Arguments.FirstOrDefault();
        var groupTypeName = typeArg?.ToString() ?? "DevOnly";

        var diagnostic = Diagnostic.Create(
            WrongNamespaceRule,
            devOnlyGroupCall.GetLocation(),
            classSymbol.Name,
            groupTypeName);
        context.ReportDiagnostic(diagnostic);
      }
    }

    private static bool IsEndpoint(INamedTypeSymbol classSymbol)
    {
      var baseType = classSymbol.BaseType;
      while (baseType != null)
      {
        // Check if it's from FastEndpoints namespace and is an Endpoint base class
        if (baseType.ContainingNamespace?.ToDisplayString() == "FastEndpoints" &&
            baseType.Name.StartsWith("Endpoint"))
        {
          return true;
        }
        baseType = baseType.BaseType;
      }
      return false;
    }

    private static bool IsGroupInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel)
    {
      if (invocation.Expression is not GenericNameSyntax genericName)
      {
        return false;
      }

      if (genericName.Identifier.Text != "Group")
      {
        return false;
      }

      // Verify it's the FastEndpoints Group method
      var symbolInfo = semanticModel.GetSymbolInfo(invocation);
      var method = symbolInfo.Symbol as IMethodSymbol;

      return method != null &&
             method.ContainingType?.ContainingNamespace?.ToDisplayString() == "FastEndpoints";
    }

    private static bool IsDevOnlyOrSubGroupOfDevOnly(ITypeSymbol typeSymbol)
    {
      // Check if it's DevOnly itself
      if (typeSymbol.Name == "DevOnly" &&
          typeSymbol.ContainingNamespace?.ToDisplayString()?.StartsWith("Server.Web.DevOnly") == true)
      {
        return true;
      }

      // Check if it inherits from SubGroup<DevOnly>
      var baseType = (typeSymbol as INamedTypeSymbol)?.BaseType;
      while (baseType != null)
      {
        // Check if base type is SubGroup<T>
        if (baseType.Name == "SubGroup" && baseType.IsGenericType)
        {
          var typeArg = baseType.TypeArguments.FirstOrDefault();
          if (typeArg != null &&
              typeArg.Name == "DevOnly" &&
              typeArg.ContainingNamespace?.ToDisplayString()?.StartsWith("Server.Web.DevOnly") == true)
          {
            return true;
          }
        }
        baseType = baseType.BaseType;
      }

      return false;
    }
  }
}
