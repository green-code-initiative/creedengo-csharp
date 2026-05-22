namespace Creedengo.Core.Analyzers;

/// <summary>GCI93: Return Task directly.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnTaskDirectly : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> MethodDeclarations = ImmutableArray.Create(SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement);

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
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(AnalyzeSyntaxNode, MethodDeclarations);
    }

    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var (modifiers, returnType, expressionBody, body) = context.Node switch
        {
            MethodDeclarationSyntax m => (m.Modifiers, m.ReturnType, m.ExpressionBody, m.Body),
            LocalFunctionStatementSyntax l => (l.Modifiers, l.ReturnType, l.ExpressionBody, l.Body),
            _ => default
        };

        // Check if the method is async
        int asyncIndex = modifiers.IndexOf(SyntaxKind.AsyncKeyword);
        if (asyncIndex == -1) return;

        // Check if the method contains a single await statement
        var awaitExpr = expressionBody?.Expression as AwaitExpressionSyntax;
        if (awaitExpr is null && body?.Statements.SingleOrDefaultNoThrow() is { } statement)
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

        // Don't report if removing 'async' would change the return type. The fix replaces 'await x' (or 'await x.ConfigureAwait(...)')
        // with 'return x', so x's type must already match the method's return type. Otherwise (e.g. 'async ValueTask' awaiting
        // a Task) the suggested fix would not compile.
        if (!ReturnTypeMatchesAwaitedExpression(context.SemanticModel, returnType, awaitExpr, context.CancellationToken))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, modifiers[asyncIndex].GetLocation()));
    }

    private static bool ReturnTypeMatchesAwaitedExpression(SemanticModel semanticModel, TypeSyntax methodReturnType, AwaitExpressionSyntax awaitExpr, System.Threading.CancellationToken cancellationToken)
    {
        if (semanticModel.GetSymbolInfo(methodReturnType, cancellationToken).Symbol is not ITypeSymbol returnType)
            return false;

        // The fixer strips a trailing .ConfigureAwait(...) before returning, so compare against the receiver's type when present.
        var expressionToReturn = awaitExpr.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "ConfigureAwait" } memberAccess }
            ? memberAccess.Expression
            : awaitExpr.Expression;

        // Compare wrappers only (Task vs ValueTask) and let the compiler vouch for type-argument compatibility.
        var expressionType = semanticModel.GetTypeInfo(expressionToReturn, cancellationToken).Type;
        return expressionType is not null && SymbolEqualityComparer.Default.Equals(returnType.OriginalDefinition, expressionType.OriginalDefinition);
    }
}
