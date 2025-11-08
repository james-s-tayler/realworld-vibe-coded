using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.MediatR
{
  /// <summary>
  /// Analyzer that bans try-catch blocks in use case handlers.
  /// Exception handling should be delegated to the ExceptionHandlingBehavior in the MediatR pipeline.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class BanTryCatchInUseCasesAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV014";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Try-catch blocks are not allowed in use case handlers",
        "Do not use try-catch blocks in use case handlers. Exception handling is automatically performed by ExceptionHandlingBehavior in the MediatR pipeline. Remove the try-catch and let exceptions bubble up.",
        "Usage",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Try-catch blocks should not be used in use case handlers (ICommandHandler or IQueryHandler implementations). The ExceptionHandlingBehavior in the MediatR pipeline automatically catches exceptions and converts them to Result.CriticalError or Result.Conflict as appropriate. Exceptions should bubble up naturally.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var compilation = compilationContext.Compilation;

        // Get the ICommandHandler and IQueryHandler interface symbols
        var commandHandlerInterface = compilation.GetTypeByMetadataName("Server.SharedKernel.MediatR.ICommandHandler`2");
        var queryHandlerInterface = compilation.GetTypeByMetadataName("Server.SharedKernel.MediatR.IQueryHandler`2");

        if (commandHandlerInterface == null && queryHandlerInterface == null)
        {
          return; // Interfaces not found, nothing to analyze
        }

        compilationContext.RegisterSyntaxNodeAction(context =>
        {
          AnalyzeTryCatch(context, commandHandlerInterface, queryHandlerInterface);
        }, SyntaxKind.TryStatement);
      });
    }

    private static void AnalyzeTryCatch(
      SyntaxNodeAnalysisContext context,
      INamedTypeSymbol commandHandlerInterface,
      INamedTypeSymbol queryHandlerInterface)
    {
      var tryStatement = (TryStatementSyntax)context.Node;

      // Find the containing type
      var containingType = tryStatement.Ancestors()
        .OfType<ClassDeclarationSyntax>()
        .FirstOrDefault();

      if (containingType == null)
      {
        return;
      }

      // Get the semantic model for the containing type
      var typeSymbol = context.SemanticModel.GetDeclaredSymbol(containingType);
      if (typeSymbol == null)
      {
        return;
      }

      // Check if the type implements ICommandHandler or IQueryHandler
      bool isHandler = typeSymbol.AllInterfaces.Any(i =>
      {
        if (i.IsGenericType && i.ConstructedFrom != null)
        {
          return SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, commandHandlerInterface) ||
                 SymbolEqualityComparer.Default.Equals(i.ConstructedFrom, queryHandlerInterface);
        }
        return false;
      });

      if (!isHandler)
      {
        return; // Not a handler class, skip
      }

      // Find the containing method
      var containingMethod = tryStatement.Ancestors()
        .OfType<MethodDeclarationSyntax>()
        .FirstOrDefault();

      if (containingMethod == null)
      {
        return;
      }

      // Check if this is the Handle method
      var methodSymbol = context.SemanticModel.GetDeclaredSymbol(containingMethod);
      if (methodSymbol == null || methodSymbol.Name != "Handle")
      {
        return;
      }

      // Report diagnostic for try-catch in Handle method of handler
      var diagnostic = Diagnostic.Create(Rule, tryStatement.GetLocation());
      context.ReportDiagnostic(diagnostic);
    }
  }
}
