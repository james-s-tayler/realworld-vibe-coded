using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Server.Analyzers.Persistence
{
  /// <summary>
  /// Analyzer that enforces aggregate root location rules in Domain-Driven Design.
  /// Ensures that:
  /// 1. Entities implementing IAggregateRoot are in the correct *Aggregate namespace
  /// 2. Each *Aggregate directory contains only one aggregate root
  /// </summary>
  [DiagnosticAnalyzer(LanguageNames.CSharp)]
  public class AggregateRootLocationAnalyzer : DiagnosticAnalyzer
  {
    public const string WrongLocationDiagnosticId = "SRV016";
    public const string MultipleRootsDiagnosticId = "SRV017";

    private static readonly DiagnosticDescriptor WrongLocationRule = new DiagnosticDescriptor(
        WrongLocationDiagnosticId,
        "Aggregate root must be in matching *Aggregate namespace",
        "Aggregate root '{0}' must be in namespace 'Server.Core.{0}Aggregate' but is in '{1}'",
        "DomainDesign",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Entities implementing IAggregateRoot must reside in Server.Core.<EntityName>Aggregate where EntityName matches the entity class name.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    private static readonly DiagnosticDescriptor MultipleRootsRule = new DiagnosticDescriptor(
        MultipleRootsDiagnosticId,
        "Each *Aggregate namespace must contain only one aggregate root",
        "Namespace '{0}' contains multiple aggregate roots: {1}. Each *Aggregate namespace should contain only one aggregate root.",
        "DomainDesign",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Each *Aggregate directory/namespace must contain exactly one entity implementing IAggregateRoot to maintain clear aggregate boundaries.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(WrongLocationRule, MultipleRootsRule);

    public override void Initialize(AnalysisContext context)
    {
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.EnableConcurrentExecution();

      // Register compilation action to analyze all aggregate roots together
      context.RegisterCompilationAction(AnalyzeCompilation);
    }

    private static void AnalyzeCompilation(CompilationAnalysisContext context)
    {
      var aggregateRootsByNamespace = new Dictionary<string, List<INamedTypeSymbol>>();

      // Find all types in the compilation
      var visitor = new AggregateRootVisitor(context, aggregateRootsByNamespace);
      visitor.Visit(context.Compilation.GlobalNamespace);

      // Check for multiple aggregate roots in the same namespace
      foreach (var kvp in aggregateRootsByNamespace)
      {
        var namespaceName = kvp.Key;
        var aggregateRoots = kvp.Value;

        if (aggregateRoots.Count > 1)
        {
          var rootNames = string.Join(", ", aggregateRoots.Select(r => r.Name));

          foreach (var root in aggregateRoots)
          {
            var location = root.Locations.FirstOrDefault();
            if (location != null)
            {
              var diagnostic = Diagnostic.Create(
                  MultipleRootsRule,
                  location,
                  namespaceName,
                  rootNames);
              context.ReportDiagnostic(diagnostic);
            }
          }
        }
      }
    }

    private class AggregateRootVisitor : SymbolVisitor
    {
      private readonly CompilationAnalysisContext _context;
      private readonly Dictionary<string, List<INamedTypeSymbol>> _aggregateRootsByNamespace;

      public AggregateRootVisitor(
          CompilationAnalysisContext context,
          Dictionary<string, List<INamedTypeSymbol>> aggregateRootsByNamespace)
      {
        _context = context;
        _aggregateRootsByNamespace = aggregateRootsByNamespace;
      }

      public override void VisitNamespace(INamespaceSymbol symbol)
      {
        foreach (var member in symbol.GetMembers())
        {
          member.Accept(this);
        }
      }

      public override void VisitNamedType(INamedTypeSymbol symbol)
      {
        // Check if this type implements IAggregateRoot
        if (!ImplementsIAggregateRoot(symbol))
        {
          return;
        }

        // Only analyze types in Server.Core namespace
        var namespaceName = symbol.ContainingNamespace?.ToDisplayString() ?? string.Empty;
        if (!namespaceName.StartsWith("Server.Core.") || !namespaceName.Contains("Aggregate"))
        {
          // If it implements IAggregateRoot but not in a Core namespace with Aggregate, that's still wrong
          if (namespaceName.StartsWith("Server.Core."))
          {
            var location = symbol.Locations.FirstOrDefault();
            if (location != null)
            {
              var diagnostic = Diagnostic.Create(
                  WrongLocationRule,
                  location,
                  symbol.Name,
                  namespaceName);
              _context.ReportDiagnostic(diagnostic);
            }
          }
          return;
        }

        // Track aggregate roots by namespace for multiple roots check
        if (!_aggregateRootsByNamespace.ContainsKey(namespaceName))
        {
          _aggregateRootsByNamespace[namespaceName] = new List<INamedTypeSymbol>();
        }
        _aggregateRootsByNamespace[namespaceName].Add(symbol);

        // Check if the entity is in the correct namespace
        // Expected: Server.Core.<EntityName>Aggregate
        var expectedNamespace = $"Server.Core.{symbol.Name}Aggregate";

        if (namespaceName != expectedNamespace)
        {
          var location = symbol.Locations.FirstOrDefault();
          if (location != null)
          {
            var diagnostic = Diagnostic.Create(
                WrongLocationRule,
                location,
                symbol.Name,
                namespaceName);
            _context.ReportDiagnostic(diagnostic);
          }
        }
      }

      private static bool ImplementsIAggregateRoot(INamedTypeSymbol type)
      {
        foreach (var @interface in type.AllInterfaces)
        {
          if (@interface.Name == "IAggregateRoot" &&
              @interface.ContainingNamespace?.ToDisplayString() == "Server.SharedKernel.Persistence")
          {
            return true;
          }
        }
        return false;
      }
    }
  }
}
