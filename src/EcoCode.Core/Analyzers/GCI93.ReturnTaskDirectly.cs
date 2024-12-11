namespace EcoCode.Analyzers;

/// <summary>GCI93: Return Task directly.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnTaskDirectly : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> MethodDeclarations = [SyntaxKind.MethodDeclaration];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI93_ReturnTaskDirectly,
        title: "Consider returning Task directly",
        message: "Consider returning a Task directly instead of awaiting a single statement",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Info,
        description: "Consider returning a Task directly instead of awaiting a single statement, as this can save performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeSyntaxNode(context), MethodDeclarations);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        // Check if the method is async
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        int asyncIndex = methodDeclaration.Modifiers.IndexOf(SyntaxKind.AsyncKeyword);
        if (asyncIndex == -1) return;

        // Check if the method contains a single await statement
        var awaitExpr = methodDeclaration.ExpressionBody?.Expression as AwaitExpressionSyntax;
        if (awaitExpr is null && methodDeclaration.Body?.Statements.SingleOrDefaultNoThrow() is { } statement)
        {
            if (statement is ExpressionStatementSyntax expressionStmt) // Is it an 'await' statement
                awaitExpr = expressionStmt.Expression as AwaitExpressionSyntax;
            else if (statement is ReturnStatementSyntax returnStmt) // Is it a 'return await' statement
                awaitExpr = returnStmt.Expression as AwaitExpressionSyntax;
        }
        if (awaitExpr is null) return;

        // Check if the await statement has any nested await statement (like parameters)
        foreach (var node in awaitExpr.DescendantNodes())
            if (node is AwaitExpressionSyntax) return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, methodDeclaration.Modifiers[asyncIndex].GetLocation()));
    }
}
