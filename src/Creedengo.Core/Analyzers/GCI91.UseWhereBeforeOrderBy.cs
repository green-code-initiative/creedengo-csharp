namespace Creedengo.Analyzers;

/// <summary>GCI91: Use Where before OrderBy.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseWhereBeforeOrderBy : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> InvocationExpressions = [SyntaxKind.InvocationExpression];
    private static readonly ImmutableArray<SyntaxKind> QueryExpressions = [SyntaxKind.QueryExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI91_UseWhereBeforeOrderBy,
        title: "Use Where before OrderBy",
        message: "Call Where before OrderBy in a LINQ method chain",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Use the Where clause before the OrderBy clause to avoid sorting unnecessary elements.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeMethodSyntax(context), InvocationExpressions);
        context.RegisterSyntaxNodeAction(static context => AnalyzeQuerySyntax(context), QueryExpressions);
    }

    private static void AnalyzeMethodSyntax(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        if (invocation.Expression is MemberAccessExpressionSyntax { Name.Identifier.Text: "Where" } memberAccess &&
            memberAccess.Expression is InvocationExpressionSyntax
            {
                Expression: MemberAccessExpressionSyntax
                {
                    Name.Identifier.Text: "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending"
                }
            })
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.Name.GetLocation()));
        }
    }

    private static void AnalyzeQuerySyntax(SyntaxNodeAnalysisContext context)
    {
        var clauses = ((QueryExpressionSyntax)context.Node).Body.Clauses;
        for (int i = 1; i < clauses.Count; i++) // Can never warn on the first clause
        {
            if (clauses[i] is WhereClauseSyntax whereClause && clauses[i - 1] is OrderByClauseSyntax)
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, whereClause.WhereKeyword.GetLocation()));
        }
    }
}
