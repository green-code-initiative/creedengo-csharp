namespace Creedengo.Core.Analyzers;

/// <summary>GCI91 fixer: Use Where before OrderBy.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseWhereBeforeOrderByFixer)), Shared]
public sealed class UseWhereBeforeOrderByFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseWhereBeforeOrderBy.Descriptor.Id];

    /// <inheritdoc/>
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false) is not { } root)
            return;

        foreach (var diagnostic in context.Diagnostics)
        {
            if (root.FindToken(diagnostic.Location.SourceSpan.Start).Parent is not { } parent)
                continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is InvocationExpressionSyntax invocation)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Use Where before OrderBy",
                            createChangedDocument: _ => RefactorMethodSyntaxAsync(context.Document, invocation),
                            equivalenceKey: "Use Where before OrderBy"),
                        diagnostic);
                    break;
                }
                if (node is QueryExpressionSyntax query)
                {
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Use Where before OrderBy",
                            createChangedDocument: _ => RefactorQuerySyntaxAsync(context.Document, query),
                            equivalenceKey: "Use Where before OrderBy"),
                        diagnostic);
                    break;
                }
            }
        }
    }

    private static async Task<Document> RefactorMethodSyntaxAsync(Document document, InvocationExpressionSyntax whereInvocation)
    {
        if (whereInvocation.Expression is not MemberAccessExpressionSyntax whereMemberAccess)
            return document;

        var sortMethods = new List<InvocationExpressionSyntax>();
        var currentInvocation = whereMemberAccess.Expression as InvocationExpressionSyntax;
        while (currentInvocation?.Expression is MemberAccessExpressionSyntax memberAccess &&
            memberAccess.Name.Identifier.Text is "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending")
        {
            sortMethods.Add(currentInvocation);
            currentInvocation = memberAccess.Expression as InvocationExpressionSyntax;
        }
        if (sortMethods.Count == 0) return document;

        sortMethods.Reverse();

        var newSortChain = whereInvocation.WithExpression(whereMemberAccess
            .WithExpression(((MemberAccessExpressionSyntax)sortMethods[0].Expression).Expression));

        foreach (var sortInvocation in sortMethods)
            newSortChain = sortInvocation.WithExpression(((MemberAccessExpressionSyntax)sortInvocation.Expression).WithExpression(newSortChain));

        return await document.WithUpdatedRoot(whereInvocation, newSortChain).ConfigureAwait(false);
    }

    private static async Task<Document> RefactorQuerySyntaxAsync(Document document, QueryExpressionSyntax query)
    {
        var clauses = query.Body.Clauses;
        for (int i = 0; i < clauses.Count - 1; i++)
        {
            if (clauses[i] is not OrderByClauseSyntax) continue;

            for (int j = i + 1; j < clauses.Count; j++) // To handle multiple OrderBy followed by a Where
            {
                var nextClause = clauses[j];
                if (nextClause is WhereClauseSyntax whereClause)
                {
                    var newClauses = clauses.ToList();
                    newClauses.RemoveAt(j);
                    newClauses.Insert(i, whereClause);
                    return await document.WithUpdatedRoot(query, query.WithBody(query.Body.WithClauses(SyntaxFactory.List(newClauses)))).ConfigureAwait(false);
                }
                if (nextClause is not OrderByClauseSyntax)
                {
                    i = j; // Skip processed clauses
                    break;
                }
            }
        }
        return document;
    }
}
