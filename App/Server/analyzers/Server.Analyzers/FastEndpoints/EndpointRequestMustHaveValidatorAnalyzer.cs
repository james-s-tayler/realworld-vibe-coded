using System.Collections.Concurrent;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.FastEndpoints
{
  /// <summary>
  /// Analyzer that requires every Endpoint&lt;TRequest, ...&gt; to have a matching FluentValidation validator.
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class EndpointRequestMustHaveValidatorAnalyzer : DiagnosticAnalyzer
  {
    public const string DiagnosticId = "SRV020";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
        "Endpoint request must have a FluentValidation validator",
        "Endpoint request '{0}' has no matching FluentValidation validator. Create a class inheriting 'AbstractValidator<{0}>' (e.g. FastEndpoints 'Validator<{0}>').",
        "Architecture",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every FastEndpoints endpoint whose request type is non-empty must be paired with a FluentValidation validator. FastEndpoints.EmptyRequest is exempt.",
        customTags: new[] { WellKnownDiagnosticTags.CompilationEnd });

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      context.RegisterCompilationStartAction(compilationContext =>
      {
        var compilation = compilationContext.Compilation;
        var emptyRequestType = compilation.GetTypeByMetadataName("FastEndpoints.EmptyRequest");
        var abstractValidatorType = compilation.GetTypeByMetadataName("FluentValidation.AbstractValidator`1");

        if (abstractValidatorType == null)
        {
          return;
        }

        var validatedRequestTypes = new ConcurrentDictionary<ITypeSymbol, byte>(SymbolEqualityComparer.Default);
        var endpointsToCheck = new ConcurrentBag<(INamedTypeSymbol RequestType, Location Location)>();

        compilationContext.RegisterSymbolAction(symbolContext =>
        {
          var symbol = (INamedTypeSymbol)symbolContext.Symbol;

          if (symbol.IsAbstract)
          {
            return;
          }

          var validatedRequest = GetValidatedRequestType(symbol, abstractValidatorType);
          if (validatedRequest != null)
          {
            validatedRequestTypes.TryAdd(validatedRequest, 0);
          }

          if (TryGetEndpointRequestType(symbol, emptyRequestType, out var requestType, out var location))
          {
            endpointsToCheck.Add((requestType, location));
          }
        }, SymbolKind.NamedType);

        compilationContext.RegisterCompilationEndAction(endContext =>
        {
          foreach (var (requestType, location) in endpointsToCheck)
          {
            if (!validatedRequestTypes.ContainsKey(requestType))
            {
              endContext.ReportDiagnostic(Diagnostic.Create(Rule, location, requestType.Name));
            }
          }
        });
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

    private static bool TryGetEndpointRequestType(
      INamedTypeSymbol symbol,
      INamedTypeSymbol emptyRequestType,
      out INamedTypeSymbol requestType,
      out Location location)
    {
      requestType = null;
      location = null;

      var baseType = symbol.BaseType;
      while (baseType != null)
      {
        if (baseType.ContainingNamespace?.ToDisplayString() == "FastEndpoints" &&
            baseType.Name == "Endpoint" &&
            baseType.TypeArguments.Length >= 1)
        {
          var firstArg = baseType.TypeArguments[0] as INamedTypeSymbol;
          if (firstArg == null)
          {
            return false;
          }

          if (emptyRequestType != null && SymbolEqualityComparer.Default.Equals(firstArg, emptyRequestType))
          {
            return false;
          }

          requestType = firstArg;
          location = symbol.Locations.Length > 0 ? symbol.Locations[0] : Location.None;
          return true;
        }

        baseType = baseType.BaseType;
      }

      return false;
    }
  }
}
