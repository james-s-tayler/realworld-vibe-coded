using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Analyzer that requires validators of IPaginatedRequest types to inherit PaginationAwareValidator&lt;T&gt;.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class PaginatedRequestValidatorBaseAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV021";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Paginated request validator must extend PaginationAwareValidator<T>",
        "Validator for paginated request '{0}' must inherit from PaginationAwareValidator<{0}>. This centralizes the Limit/Offset validation rules (1..100 / >=0).",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Validators for request types that implement IPaginatedRequest must inherit Server.Web.Shared.Pagination.PaginationAwareValidator<T> to share a single Limit/Offset rule definition.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var compilation = compilationContext.Compilation;
        var abstractValidatorType = compilation.GetTypeByMetadataName("FluentValidation.AbstractValidator`1");
        var paginatedRequestInterface = compilation.GetTypeByMetadataName("Server.SharedKernel.Pagination.IPaginatedRequest");
        var paginationAwareValidator = compilation.GetTypeByMetadataName("Server.Web.Shared.Pagination.PaginationAwareValidator`1");

        if (abstractValidatorType == null || paginatedRequestInterface == null || paginationAwareValidator == null)
        {
          return;
        }

        compilationContext.RegisterSymbolAction(symbolContext =>
        {
          var symbol = (INamedTypeSymbol)symbolContext.Symbol;

          if (symbol.IsAbstract)
          {
            return;
          }

          var validatedRequest = GetValidatedRequestType(symbol, abstractValidatorType);
          if (validatedRequest == null)
          {
            return;
          }

          if (!ImplementsInterface(validatedRequest, paginatedRequestInterface))
          {
            return;
          }

          if (InheritsFrom(symbol, paginationAwareValidator))
          {
            return;
          }

          var location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
          symbolContext.ReportDiagnostic(Diagnostic.Create(Rule, location, validatedRequest.Name));
        }, SymbolKind.NamedType);
      });
    }

    private static ITypeSymbol GetValidatedRequestType(INamedTypeSymbol symbol, INamedTypeSymbol abstractValidatorType)
    {
      var baseType = symbol.BaseType;
      while (baseType != null)
      {
        if (SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, abstractValidatorType))
        {
          if (baseType.TypeArguments.Length == 1)
          {
            return baseType.TypeArguments[0];
          }
        }

        baseType = baseType.BaseType;
      }

      return null;
    }

    private static bool ImplementsInterface(ITypeSymbol type, INamedTypeSymbol interfaceType)
    {
      foreach (var iface in type.AllInterfaces)
      {
        if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, interfaceType))
        {
          return true;
        }
      }

      return false;
    }

    private static bool InheritsFrom(INamedTypeSymbol symbol, INamedTypeSymbol targetDefinition)
    {
      var baseType = symbol.BaseType;
      while (baseType != null)
      {
        if (SymbolEqualityComparer.Default.Equals(baseType.OriginalDefinition, targetDefinition))
        {
          return true;
        }

        baseType = baseType.BaseType;
      }

      return false;
    }
  }
}
