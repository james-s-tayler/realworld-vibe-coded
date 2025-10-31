using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers
{
  /// <summary>
  /// Analyzer that bans Result&lt;Unit&gt; usage.
  /// Use case handlers should return Result&lt;T&gt; with a proper domain entity type instead of Unit.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanResultUnitAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV015";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Result<Unit> is not allowed",
        "Do not use Result<Unit>. Use Result<T> with a proper domain entity type instead. For delete operations that don't need to return data, use Result<T>.NoContent() where T is the entity being deleted.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Result<Unit> should not be used. Instead, use Result<T> where T is a proper domain entity type. For operations that don't return meaningful data (like deletes), still use Result<T> and call NoContent() to return HTTP 204.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var compilation = compilationContext.Compilation;

        // Get the Result<T> type symbol
        var resultType = compilation.GetTypeByMetadataName("Server.SharedKernel.Result.Result`1");
        var unitType = compilation.GetTypeByMetadataName("MediatR.Unit");

        if (resultType == null || unitType == null)
        {
          return; // Types not found, nothing to analyze
        }

        compilationContext.RegisterSyntaxNodeAction(context =>
        {
          AnalyzeGenericName(context, resultType, unitType);
        }, SyntaxKind.GenericName);
      });
    }

    private static void AnalyzeGenericName(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol resultType,
      INamedTypeSymbol unitType)
    {
      var genericName = (GenericNameSyntax)context.Node;

      // Get the symbol for the generic name
      var symbolInfo = context.SemanticModel.GetSymbolInfo(genericName);
      var typeSymbol = symbolInfo.Symbol as INamedTypeSymbol;

      if (typeSymbol == null)
      {
        return;
      }

      // Check if this is Result<T>
      if (!SymbolEqualityComparer.Default.Equals(typeSymbol.OriginalDefinition, resultType))
      {
        return;
      }

      // Check if T is Unit
      if (typeSymbol.TypeArguments.Length == 1)
      {
        var typeArgument = typeSymbol.TypeArguments[0];
        if (SymbolEqualityComparer.Default.Equals(typeArgument, unitType))
        {
          // Report diagnostic for Result<Unit>
          var diagnostic = Diagnostic.Create(Rule, genericName.GetLocation());
          context.ReportDiagnostic(diagnostic);
        }
      }
    }
  }
}
