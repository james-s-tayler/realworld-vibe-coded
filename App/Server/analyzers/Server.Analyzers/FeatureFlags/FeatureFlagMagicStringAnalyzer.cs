using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FeatureFlags
{
  /// <summary>
  /// Bans string literals as arguments to IFeatureManager.IsEnabledAsync() and IFeatureFlagService.IsEnabledAsync().
  /// Use FeatureFlags.* constants instead.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class FeatureFlagMagicStringAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "FF001";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Feature flag name must use FeatureFlags constants",
        "Use FeatureFlags.* constants instead of string literals for feature flag names",
        "FeatureFlags",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "String literals passed to IsEnabledAsync are fragile and bypass compile-time validation. Use constants from Server.SharedKernel.FeatureFlags.FeatureFlags.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var assemblyName = compilationContext.Compilation.AssemblyName ?? "";

        if (assemblyName.Contains("Test"))
        {
          return;
        }

        compilationContext.RegisterSyntaxNodeAction(
            AnalyzeInvocation,
            SyntaxKind.InvocationExpression);
      });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
      var invocation = (InvocationExpressionSyntax)context.Node;

      if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
      {
        return;
      }

      if (memberAccess.Name.Identifier.Text != "IsEnabledAsync")
      {
        return;
      }

      // Verify the receiver is IFeatureManager or IFeatureFlagService
      var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
      if (receiverType == null || !IsFeatureManagerType(receiverType))
      {
        return;
      }

      var arguments = invocation.ArgumentList.Arguments;
      if (arguments.Count == 0)
      {
        return;
      }

      var firstArg = arguments[0].Expression;

      // Only flag string literals and interpolated strings
      if (firstArg is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, firstArg.GetLocation()));
      }
      else if (firstArg is InterpolatedStringExpressionSyntax)
      {
        context.ReportDiagnostic(Diagnostic.Create(Rule, firstArg.GetLocation()));
      }
    }

    private static bool IsFeatureManagerType(ITypeSymbol type)
    {
      if (IsMatchingType(type))
      {
        return true;
      }

      foreach (var iface in type.AllInterfaces)
      {
        if (IsMatchingType(iface))
        {
          return true;
        }
      }

      return false;
    }

    private static bool IsMatchingType(ITypeSymbol type)
    {
      // IFeatureManager from Microsoft.FeatureManagement
      if (type.Name == "IFeatureManager" &&
          type.ContainingNamespace?.ToDisplayString() == "Microsoft.FeatureManagement")
      {
        return true;
      }

      // IFeatureFlagService from our codebase
      if (type.Name == "IFeatureFlagService" &&
          type.ContainingNamespace?.ToDisplayString() == "Server.SharedKernel.Interfaces")
      {
        return true;
      }

      return false;
    }
  }
}
