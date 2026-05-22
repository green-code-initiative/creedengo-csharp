namespace Creedengo.Core.Analyzers;

/// <summary>GCI93 fixer: Return Task directly.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnTaskDirectly)), Shared]
public sealed class ReturnTaskDirectlyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ImmutableArray.Create(ReturnTaskDirectly.Descriptor.Id);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not { } parent)
                continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is MethodDeclarationSyntax methodDecl)
                {
                    RegisterFixForMethod(context, diagnostic, methodDecl);
                    break;
                }
                if (node is LocalFunctionStatementSyntax localFunction)
                {
                    RegisterFixForLocalFunction(context, diagnostic, localFunction);
                    break;
                }
            }
        }
    }

    private static void RegisterFixForMethod(CodeFixContext context, Diagnostic diagnostic, MethodDeclarationSyntax methodDecl)
    {
        int asyncIndex = methodDecl.Modifiers.IndexOf(SyntaxKind.AsyncKeyword);
        if (asyncIndex == -1) return;

        // If 'async' is the first modifier (no accessibility, attributes, etc. precede it), its leading trivia is the line indentation.
        // Move that trivia onto the return type before removing the modifier; otherwise the method gets de-indented.
        var asyncToken = methodDecl.Modifiers[asyncIndex];
        var newModifiers = methodDecl.Modifiers.RemoveAt(asyncIndex);
        var newReturnType = asyncIndex == 0
            ? methodDecl.ReturnType.WithLeadingTrivia(asyncToken.LeadingTrivia.AddRange(methodDecl.ReturnType.GetLeadingTrivia()))
            : methodDecl.ReturnType;

        if (methodDecl.ExpressionBody is { Expression: AwaitExpressionSyntax awaitExpr1 })
        {
            var newExpressionBody = BuildExpressionBody(awaitExpr1, methodDecl.ExpressionBody);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Return Task directly",
                    createChangedDocument: _ => context.Document.WithUpdatedRoot(methodDecl,
                        methodDecl.WithModifiers(newModifiers).WithReturnType(newReturnType).WithExpressionBody(newExpressionBody)),
                    equivalenceKey: "Return Task directly"),
                diagnostic);
            return;
        }

        var statement = methodDecl.Body?.Statements.SingleOrDefaultNoThrow();
        if (TryBuildBlockBody(statement, methodDecl.Body, out var newBlock))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Return Task directly",
                    createChangedDocument: _ => context.Document.WithUpdatedRoot(methodDecl,
                        methodDecl.WithModifiers(newModifiers).WithReturnType(newReturnType).WithBody(newBlock)),
                    equivalenceKey: "Return Task directly"),
                diagnostic);
        }
    }

    private static void RegisterFixForLocalFunction(CodeFixContext context, Diagnostic diagnostic, LocalFunctionStatementSyntax localFunction)
    {
        int asyncIndex = localFunction.Modifiers.IndexOf(SyntaxKind.AsyncKeyword);
        if (asyncIndex == -1) return;

        // For local functions, 'async' is typically the first token, so its leading trivia carries the line indentation.
        // Move that trivia onto the return type before removing the modifier; otherwise the function gets de-indented.
        var asyncToken = localFunction.Modifiers[asyncIndex];
        var newModifiers = localFunction.Modifiers.RemoveAt(asyncIndex);
        var newReturnType = asyncIndex == 0
            ? localFunction.ReturnType.WithLeadingTrivia(asyncToken.LeadingTrivia.AddRange(localFunction.ReturnType.GetLeadingTrivia()))
            : localFunction.ReturnType;

        if (localFunction.ExpressionBody is { Expression: AwaitExpressionSyntax awaitExpr1 })
        {
            var newExpressionBody = BuildExpressionBody(awaitExpr1, localFunction.ExpressionBody);
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Return Task directly",
                    createChangedDocument: _ => context.Document.WithUpdatedRoot(localFunction,
                        localFunction.WithModifiers(newModifiers).WithReturnType(newReturnType).WithExpressionBody(newExpressionBody)),
                    equivalenceKey: "Return Task directly"),
                diagnostic);
            return;
        }

        var statement = localFunction.Body?.Statements.SingleOrDefaultNoThrow();
        if (TryBuildBlockBody(statement, localFunction.Body, out var newBlock))
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Return Task directly",
                    createChangedDocument: _ => context.Document.WithUpdatedRoot(localFunction,
                        localFunction.WithModifiers(newModifiers).WithReturnType(newReturnType).WithBody(newBlock)),
                    equivalenceKey: "Return Task directly"),
                diagnostic);
        }
    }

    private static ExpressionSyntax GetExpressionToReturn(AwaitExpressionSyntax awaitExpr) =>
        awaitExpr.Expression is InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax { Name.Identifier.Text: "ConfigureAwait" } memberAccess }
        ? memberAccess.Expression // If it is a ConfigureAwait call, strip it for the return statement
        : awaitExpr.Expression; // Else keep the expression as is

    private static ArrowExpressionClauseSyntax BuildExpressionBody(AwaitExpressionSyntax awaitExpr, ArrowExpressionClauseSyntax originalArrow) =>
        SyntaxFactory.ArrowExpressionClause(GetExpressionToReturn(awaitExpr).WithTriviaFrom(awaitExpr))
            .WithTriviaFrom(originalArrow);

    private static bool TryBuildBlockBody(StatementSyntax? statement, BlockSyntax? originalBody, out BlockSyntax newBlock)
    {
        if (originalBody is not null && statement is ExpressionStatementSyntax { Expression: AwaitExpressionSyntax awaitExpr } exprStmt)
        {
            var newReturnStmt = SyntaxFactory.ReturnStatement(GetExpressionToReturn(awaitExpr))
                .WithLeadingTrivia(awaitExpr.GetLeadingTrivia())
                .WithTrailingTrivia(exprStmt.SemicolonToken.TrailingTrivia);
            newBlock = WrapInBlock(newReturnStmt, originalBody);
            return true;
        }

        if (originalBody is not null && statement is ReturnStatementSyntax { Expression: AwaitExpressionSyntax awaitExpr2 } returnStmt)
        {
            var newReturnStmt = returnStmt.WithExpression(GetExpressionToReturn(awaitExpr2));
            newBlock = WrapInBlock(newReturnStmt, originalBody);
            return true;
        }

        newBlock = null!;
        return false;
    }

    private static BlockSyntax WrapInBlock(StatementSyntax statement, BlockSyntax originalBody) =>
        SyntaxFactory.Block(statement)
            .WithOpenBraceToken(originalBody.OpenBraceToken)
            .WithCloseBraceToken(originalBody.CloseBraceToken)
            .WithTriviaFrom(originalBody);
}
