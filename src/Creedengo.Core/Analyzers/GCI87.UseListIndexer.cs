using System.Collections;

namespace Creedengo.Core.Analyzers;

/// <summary>GCI87: Use list indexer.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseListIndexer : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = ImmutableArray.Create(SyntaxKind.InvocationExpression);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI87_UseCollectionIndexer,
        title: "Use list indexer",
        message: "A list indexer should be used instead of a Linq method",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Collections that implement IList, IList<T> or IReadOnlyList<T>, should use their indexers instead of Linq methods for improved performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static startContext =>
        {
            // Resolve well-known types once per compilation rather than on every InvocationExpression.
            var enumerableType = startContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if (enumerableType is null) return; // No System.Linq referenced; nothing this analyzer can flag.

            var iReadOnlyListT = startContext.Compilation.GetTypeByMetadataName(typeof(IReadOnlyList<>).FullName);
            var iListT = startContext.Compilation.GetTypeByMetadataName(typeof(IList<>).FullName);
            var iList = startContext.Compilation.GetTypeByMetadataName(typeof(IList).FullName);

            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocationExpression(nodeContext, enumerableType, iReadOnlyListT, iListT, iList),
                SyntaxKinds);
        });
    }

    // TODO: analysis can be improved by including scenarios with method chains
    // For example: myList.Skip(5).First() should be refactored to myList[5]

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumerableType, INamedTypeSymbol? iReadOnlyListT, INamedTypeSymbol? iListT, INamedTypeSymbol? iList)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;
        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess ||
            context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol is not IMethodSymbol method ||
            !method.IsExtensionMethod ||
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, enumerableType))
        {
            return;
        }

        bool report = method.Name switch
        {
            nameof(Enumerable.First) => method.Parameters.Length == 0,
            nameof(Enumerable.Last) => method.Parameters.Length == 0,
            nameof(Enumerable.ElementAt) => method.Parameters.Length == 1,
            _ => false,
        };

        if (report && IsList(context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type, iReadOnlyListT, iListT, iList))
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.GetLocation()));

        static bool IsList(ITypeSymbol? type, INamedTypeSymbol? iReadOnlyListT, INamedTypeSymbol? iListT, INamedTypeSymbol? iList)
        {
            if (type is null) return false;

            foreach (var iface in type.AllInterfaces)
            {
                if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, iReadOnlyListT) ||
                    SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, iListT) ||
                    SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, iList))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
