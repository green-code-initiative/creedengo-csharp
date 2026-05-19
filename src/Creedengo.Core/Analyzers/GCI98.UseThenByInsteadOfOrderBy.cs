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
        context.RegisterSyntaxNodeAction(static context => AnalyzeNode(context), InvocationExpressions);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
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

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.Name.GetLocation()));
    }
}
