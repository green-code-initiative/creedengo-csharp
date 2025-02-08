using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Creedengo.Core.Analyzers;

/// <summary>GCI90 fixer: Use Cast instead of Select to cast.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseCastInsteadOfSelectFixer)), Shared]
public sealed class UseCastInsteadOfSelectFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseCastInsteadOfSelect.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.FirstOrDefault() is not { } diagnostic ||
            await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root ||
            root.FindNode(context.Span, getInnermostNodeForTie: true) is not { } nodeToFix)
        {
            return;
        }

        // If the so-called nodeToFix is a Name (most likely a method name such as 'Select' or 'Count'),
        // adjust it so that it refers to its InvocationExpression ancestor instead.
        if ((nodeToFix.IsKind(SyntaxKind.IdentifierName) || nodeToFix.IsKind(SyntaxKind.GenericName)) && !TryGetInvocationExpressionAncestor(ref nodeToFix))
            return;

        context.RegisterCodeFix(CodeAction.Create(
            "Use Cast instead of Select",
            ct => RefactorAsync(context.Document, nodeToFix, ct),
            equivalenceKey: "Use Cast instead of Select"),
            context.Diagnostics);
    }

    private static bool TryGetInvocationExpressionAncestor(ref SyntaxNode nodeToFix)
    {
        var node = nodeToFix;
        while (node is not null)
        {
            if (node.IsKind(SyntaxKind.InvocationExpression))
            {
                nodeToFix = node;
                return true;
            }
            node = node.Parent;
        }
        return false;
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode nodeToFix, CancellationToken cancellationToken)
    {
        if (nodeToFix is not InvocationExpressionSyntax { Expression: MemberAccessExpressionSyntax memberAccessExpr } selectInvocationExpr)
            return document;

        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        if (editor.SemanticModel.GetOperation(selectInvocationExpr, cancellationToken) is not IInvocationOperation operation)
            return document;

        var generator = editor.Generator;
        var typeSyntax =
            // Check if we have a cast expression in the lambda
            selectInvocationExpr.ArgumentList.Arguments[0].Expression is LambdaExpressionSyntax lambda &&
            lambda.Body is CastExpressionSyntax castExpr
            ? castExpr.Type
            // Otherwise, check if we have explicit type parameters
            : memberAccessExpr.Name is GenericNameSyntax genericName
            ? genericName.TypeArgumentList.Arguments[1]
            // Fallback to generating type syntax from the semantic info
            : (TypeSyntax)generator.TypeExpression(operation.TargetMethod.TypeArguments[1]);

        var castNameSyntax = SyntaxFactory.GenericName(SyntaxFactory.Identifier("Cast"))
            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(typeSyntax)));

        var arguments = selectInvocationExpr.ArgumentList.Arguments;
        var castInvocationExpr = generator.InvocationExpression(
            generator.MemberAccessExpression(memberAccessExpr.Expression, castNameSyntax),
            arguments.Count < 2 ? [] : [arguments[0]]); // If args count = 2, the source is passed as the 1st argument, pass it to InvocationExpression

        editor.ReplaceNode(selectInvocationExpr, castInvocationExpr.WithAdditionalAnnotations(Simplifier.Annotation));
        return editor.GetChangedDocument();
    }
}
