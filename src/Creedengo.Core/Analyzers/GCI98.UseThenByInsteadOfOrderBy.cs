namespace Creedengo.Core.Analyzers;

/// <summary>GCI98: Use 'ThenBy' instead of 'OrderBy' in a LINQ sort chain.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseThenByInsteadOfOrderBy : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> InvocationExpressions =
        ImmutableArray.Create(SyntaxKind.InvocationExpression);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI98_UseThenByInsteadOfOrderBy,
        title: "Use 'ThenBy' instead of 'OrderBy'",
        message: "Call 'ThenBy' or 'ThenByDescending' instead of 'OrderBy' or 'OrderByDescending' to preserve the primary sort order",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Chaining 'OrderBy' or 'OrderByDescending' after another sort operation discards all previous sort keys. Use 'ThenBy' or 'ThenByDescending' to add a secondary sort key while preserving the primary sort.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
        ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static startContext =>
        {
            var enumerableType = startContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            var queryableType = startContext.Compilation.GetTypeByMetadataName("System.Linq.Queryable");
            if (enumerableType is null && queryableType is null) return;

            var expressionType = startContext.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");

            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeNode(nodeContext, enumerableType, queryableType, expressionType),
                InvocationExpressions);
        });
    }

    private static void AnalyzeNode(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? enumerableType,
        INamedTypeSymbol? queryableType,
        INamedTypeSymbol? expressionType)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        if (invocation.Expression is not MemberAccessExpressionSyntax
            { Name.Identifier.Text: "OrderBy" or "OrderByDescending" } memberAccess)
            return;

        if (memberAccess.Expression is not InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Name.Identifier.Text: "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending"
                }
            })
            return;

        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method ||
            !IsLinqSortMethod(method, enumerableType, queryableType))
        {
            return;
        }

        if (IsInsideExpressionTree(context.SemanticModel, invocation, expressionType))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.Name.GetLocation()));
    }

    private static bool IsLinqSortMethod(IMethodSymbol method, INamedTypeSymbol? enumerableType, INamedTypeSymbol? queryableType) =>
        (enumerableType is not null && SymbolEqualityComparer.Default.Equals(method.ContainingType, enumerableType)) ||
        (queryableType is not null && SymbolEqualityComparer.Default.Equals(method.ContainingType, queryableType));

    private static bool IsInsideExpressionTree(SemanticModel semanticModel, SyntaxNode node, INamedTypeSymbol? expressionType)
    {
        if (expressionType is null) return false;

        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is not LambdaExpressionSyntax and not AnonymousMethodExpressionSyntax)
                continue;

            if (semanticModel.GetTypeInfo(current).ConvertedType is INamedTypeSymbol namedType &&
                SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, expressionType))
            {
                return true;
            }
        }
        return false;
    }
}
