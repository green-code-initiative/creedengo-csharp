namespace Creedengo.Core.Analyzers;

/// <summary>
/// GCI96 fixer: Use 'EventArgs.Empty' instead of 'new EventArgs()'.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseEventArgsDotEmptyFixer)), Shared]
public sealed class UseEventArgsDotEmptyFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create(UseEventArgsDotEmpty.Descriptor.Id);

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0)
            return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var node = root.FindNode(diagnosticSpan);

            // Check if the node is ObjectCreationExpression (e.g., new EventArgs())
            if (node is ObjectCreationExpressionSyntax creation)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Use 'EventArgs.Empty'",
                        c => ReplaceWithEventArgsEmptyAsync(context.Document, creation, c),
                        "UseEventArgsDotEmpty"),
                    diagnostic);
            }

            // Check if the node is an InvocationExpression (e.g., E?.Invoke(this, new EventArgs()))
            else if (node is InvocationExpressionSyntax invocation)
            {
                var argument = invocation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                if (argument is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type.ToString() == "EventArgs")
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use 'EventArgs.Empty'",
                            c => ReplaceWithEventArgsEmptyAsync(context.Document, objectCreation, c),
                            "UseEventArgsDotEmpty"),
                        diagnostic);
                }
            }

            // Check if the node is a Lambda expression (e.g., Action a = () => E?.Invoke(this, new EventArgs()))
            else if (node is LambdaExpressionSyntax lambda)
            {
                var invocation2 = lambda.Body.DescendantNodes().OfType<InvocationExpressionSyntax>().FirstOrDefault();
                if (invocation2 != null)
                {
                    var argument = invocation2.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                    if (argument is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type.ToString() == "EventArgs")
                    {
                        context.RegisterCodeFix(
                            CodeAction.Create(
                                "Use 'EventArgs.Empty'",
                                c => ReplaceWithEventArgsEmptyAsync(context.Document, objectCreation, c),
                                "UseEventArgsDotEmpty"),
                            diagnostic);
                    }
                }
            }

            // Check for method invocations where the argument is `new EventArgs()`
            else if (node is InvocationExpressionSyntax methodInvocation)
            {
                var argument = methodInvocation.ArgumentList?.Arguments.FirstOrDefault()?.Expression;
                if (argument is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type.ToString() == "EventArgs")
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use 'EventArgs.Empty'",
                            c => ReplaceWithEventArgsEmptyAsync(context.Document, objectCreation, c),
                            "UseEventArgsDotEmpty"),
                        diagnostic);
                }
            }
            else if (node is ArgumentSyntax argument)
            {
                // Check if the argument expression is `new EventArgs()`
                if (argument.Expression is ObjectCreationExpressionSyntax objectCreation && objectCreation.Type.ToString() == "EventArgs")
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            "Use 'EventArgs.Empty'",
                            c => ReplaceWithEventArgsEmptyAsync(context.Document, objectCreation, c),
                            "UseEventArgsDotEmpty"),
                        diagnostic);
                }
            }
        }
    }

    private static async Task<Document> ReplaceWithEventArgsEmptyAsync(
        Document document,
        SyntaxNode creation,
        CancellationToken cancellationToken)
    {
        var eventArgsEmpty = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("EventArgs"),
            SyntaxFactory.IdentifierName("Empty"));

        eventArgsEmpty = eventArgsEmpty.WithTriviaFrom(creation);

        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        var newRoot = root.ReplaceNode(creation, eventArgsEmpty);
        return document.WithSyntaxRoot(newRoot);
    }

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;
}
