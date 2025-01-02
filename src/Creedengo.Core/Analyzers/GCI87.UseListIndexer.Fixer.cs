using System.Reflection;

namespace Creedengo.Core.Analyzers;

/// <summary>GCI87 fixer: Use list indexer.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseListIndexerFixer)), Shared]
public sealed class UseListIndexerFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UseListIndexer.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var parent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            if (parent is null) continue;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is not InvocationExpressionSyntax invocation || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
                    continue;

                var refactorFunc = memberAccess.Name.Identifier.ValueText switch
                {
                    nameof(Enumerable.First) => RefactorFirstAsync,
                    nameof(Enumerable.Last) =>
                        context.Document.GetLanguageVersion() >= LanguageVersion.CSharp8
                        ? RefactorLastWithIndexAsync
                        : RefactorLastWithCountOrLengthAsync,
                    nameof(Enumerable.ElementAt) => RefactorElementAtAsync,
                    _ => default(Func<Document, InvocationExpressionSyntax, CancellationToken, Task<Document>>),
                };

                if (refactorFunc is null) continue;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Use collection indexer",
                        createChangedDocument: token => refactorFunc(context.Document, invocation, token),
                        equivalenceKey: "Use collection indexer"),
                    diagnostic);
                break;
            }
        }
    }

    private static Task<Document> UpdateDocument(Document document, InvocationExpressionSyntax invocation, ExpressionSyntax indexExpr) =>
        document.WithUpdatedRoot(invocation, SyntaxFactory.ElementAccessExpression(
            ((MemberAccessExpressionSyntax)invocation.Expression).Expression,
            SyntaxFactory.BracketedArgumentList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Argument(indexExpr)))));

    private static Task<Document> RefactorFirstAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken token) =>
        UpdateDocument(document, invocationExpr, SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));

    private static Task<Document> RefactorLastWithIndexAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken token) =>
        UpdateDocument(document, invocation,
            SyntaxFactory.PrefixUnaryExpression(SyntaxKind.IndexExpression,
                SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1))));

    private static async Task<Document> RefactorLastWithCountOrLengthAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken token)
    {
        if (await document.GetSemanticModelAsync(token).ConfigureAwait(false) is not { } semanticModel)
            return document;

        var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
        if (semanticModel.GetTypeInfo(memberAccess.Expression, token).Type is not { } memberType ||
            GetCountOrLength(memberType, invocation.SpanStart, semanticModel) is not { } countOrLength)
        {
            return document;
        }

        var property = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            memberAccess.Expression,
            SyntaxFactory.IdentifierName(countOrLength.Name));

        var oneLiteral = SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(1));

        var indexExpression = SyntaxFactory.BinaryExpression(SyntaxKind.SubtractExpression, property, oneLiteral);

        return await UpdateDocument(document, invocation, indexExpression).ConfigureAwait(false);

        static ISymbol? GetCountOrLength(ITypeSymbol type, int position, SemanticModel semanticModel)
        {
            do
            {
                foreach (var member in type.GetMembers())
                {
                    if (member.Name is nameof(IReadOnlyList<int>.Count) or nameof(Array.Length) &&
                        member is IPropertySymbol prop &&
                        prop.Type.IsPrimitiveNumber() &&
                        semanticModel.IsAccessible(position, prop))
                    {
                        return prop;
                    }
                }
                type = type.BaseType!;
            } while (type is not null);

            return null;
        }
    }

    private static Task<Document> RefactorElementAtAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken token) =>
        UpdateDocument(document, invocation, invocation.ArgumentList.Arguments[0].Expression);
}
