namespace Creedengo.Core.Analyzers;

/// <summary>GCI98 fixer: Use 'ThenBy' instead of 'OrderBy'.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseThenByInsteadOfOrderByFixer)), Shared]
public sealed class UseThenByInsteadOfOrderByFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds =
        ImmutableArray.Create(UseThenByInsteadOfOrderBy.Descriptor.Id);

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true) is not SimpleNameSyntax nameSyntax)
                continue;
            if (nameSyntax.Parent is not MemberAccessExpressionSyntax memberAccess)
                continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Use 'ThenBy' instead of 'OrderBy'",
                    createChangedDocument: _ => FixAsync(context.Document, memberAccess, nameSyntax),
                    equivalenceKey: "UseThenByInsteadOfOrderBy"),
                diagnostic);
        }
    }

    private static Task<Document> FixAsync(
        Document document,
        MemberAccessExpressionSyntax memberAccess,
        SimpleNameSyntax nameSyntax)
    {
        var newIdentifier = nameSyntax.Identifier.Text == "OrderBy" ? "ThenBy" : "ThenByDescending";
        SimpleNameSyntax newNameSyntax = nameSyntax is GenericNameSyntax generic
            ? generic.WithIdentifier(SyntaxFactory.Identifier(newIdentifier))
            : SyntaxFactory.IdentifierName(newIdentifier);
        var newMemberAccess = memberAccess.WithName(newNameSyntax.WithTriviaFrom(nameSyntax));
        return document.WithUpdatedRoot(memberAccess, newMemberAccess);
    }
}
