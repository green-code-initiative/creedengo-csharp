namespace Creedengo.Core.Analyzers;

/// <summary>GCI2334: Use 'TrueForAll' instead of 'All' on a List.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TrueForAllInsteadOfAll : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = ImmutableArray.Create(SyntaxKind.InvocationExpression);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI2334_TrueForAllInsteadOfAll,
        title: "Use 'TrueForAll' instead of 'All'",
        message: "Use 'List<T>.TrueForAll' instead of 'Enumerable.All' for better performance",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Prefer 'List<T>.TrueForAll' over the LINQ 'Enumerable.All' extension method when the source is a 'List<T>', as it avoids LINQ overhead.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static context =>
        {
            var enumerableType = context.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            var listType = context.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            if (enumerableType is null || listType is null) return;

            context.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, enumerableType, listType),
                SyntaxKinds);
        });
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol enumerableType, INamedTypeSymbol listType)
    {
        var invocationExpr = (InvocationExpressionSyntax)context.Node;

        if (invocationExpr.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.Text != nameof(Enumerable.All))
            return;

        if (context.SemanticModel.GetSymbolInfo(invocationExpr).Symbol is not IMethodSymbol method)
            return;

        if (!method.IsExtensionMethod || !SymbolEqualityComparer.Default.Equals(method.ContainingType, enumerableType))
            return;

        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (receiverType is null || !SymbolEqualityComparer.Default.Equals(receiverType.OriginalDefinition, listType))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.Name.GetLocation()));
    }
}
