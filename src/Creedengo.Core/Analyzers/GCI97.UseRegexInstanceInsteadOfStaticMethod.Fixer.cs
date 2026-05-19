namespace Creedengo.Core.Analyzers;

/// <summary>GCI97 fixer: Use Regex instance instead of static method.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseRegexInstanceInsteadOfStaticMethodFixer)), Shared]
public sealed class UseRegexInstanceInsteadOfStaticMethodFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = ImmutableArray.Create(UseRegexInstanceInsteadOfStaticMethod.Descriptor.Id);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var node = root.FindNode(context.Span, getInnermostNodeForTie: true);
        if (node is not InvocationExpressionSyntax) return;

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Use Regex instance",
                createChangedDocument: token => RefactorAsync(context.Document, node, token),
                equivalenceKey: "Use Regex instance"),
            context.Diagnostics);
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode node, CancellationToken token)
    {
        var editor = await DocumentEditor.CreateAsync(document, token).ConfigureAwait(false);
        if (node is not InvocationExpressionSyntax invocation) return document;
        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess) return document;

        var methodName = memberAccess.Name.Identifier.Text;
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2) return document;

        // First arg is input, second is pattern for static Regex methods
        var inputArg = args[0];
        var patternArg = args[1];

        // Build remaining args (skip input and pattern, e.g. RegexOptions)
        var remainingArgs = new SyntaxList<ArgumentSyntax>();
        var constructorExtraArgs = new SyntaxList<ArgumentSyntax>();
        for (int i = 2; i < args.Count; i++)
        {
            var argType = editor.SemanticModel.GetTypeInfo(args[i].Expression, token).Type;
            if (argType is not null && argType.Name == "RegexOptions")
                constructorExtraArgs = constructorExtraArgs.Add(args[i]);
            else
                remainingArgs = remainingArgs.Add(args[i]);
        }

        // Build constructor arguments: pattern [, options]
        var constructorArgs = new[] { SyntaxFactory.Argument(patternArg.Expression) }
            .Concat(constructorExtraArgs.Select(a => SyntaxFactory.Argument(a.Expression)));

        // Create field: private readonly Regex _regex = new Regex(pattern);
        var fieldDeclaration = SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                SyntaxFactory.IdentifierName("Regex"),
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator("_regex")
                        .WithInitializer(SyntaxFactory.EqualsValueClause(
                            SyntaxFactory.ObjectCreationExpression(
                                SyntaxFactory.IdentifierName("Regex"),
                                SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(constructorArgs)),
                                null))))))
            .WithModifiers(SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)))
            .NormalizeWhitespace()
            .WithLeadingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
            .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed);

        // Replace invocation: _regex.IsMatch(input [, remaining])
        var instanceArgs = new[] { SyntaxFactory.Argument(inputArg.Expression) }
            .Concat(remainingArgs.Select(a => SyntaxFactory.Argument(a.Expression)));

        var newInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("_regex"),
                SyntaxFactory.IdentifierName(methodName)),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(instanceArgs)));

        editor.ReplaceNode(invocation, newInvocation);

        // Insert field before the containing method
        var containingMethod = invocation.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (containingMethod is not null)
            editor.InsertBefore(containingMethod, fieldDeclaration);

        return editor.GetChangedDocument();
    }
}
