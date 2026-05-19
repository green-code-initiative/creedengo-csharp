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

        // Detect EOL style from existing document
        var eolTrivia = editor.OriginalRoot.DescendantTrivia()
            .FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia));
        var eol = eolTrivia != default ? eolTrivia : SyntaxFactory.ElasticLineFeed;

        var methodName = memberAccess.Name.Identifier.Text;
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2) return document;

        // First arg is input, second is pattern for static Regex methods
        var inputArg = args[0];
        var patternArg = args[1];

        // Only offer fix when pattern is a constant expression (literal, const field, etc.)
        var patternConstant = editor.SemanticModel.GetConstantValue(patternArg.Expression, token);
        if (!patternConstant.HasValue) return document;

        // Build remaining args (skip input and pattern, e.g. RegexOptions, TimeSpan)
        var constructorExtraArgs = new SyntaxList<ArgumentSyntax>();
        var instanceExtraArgs = new SyntaxList<ArgumentSyntax>();
        for (int i = 2; i < args.Count; i++)
        {
            var argType = editor.SemanticModel.GetTypeInfo(args[i].Expression, token).Type;
            if (argType is not null && (argType.Name == "RegexOptions" || argType.Name == "TimeSpan"))
                constructorExtraArgs = constructorExtraArgs.Add(args[i].WithNameColon(null));
            else
                instanceExtraArgs = instanceExtraArgs.Add(args[i].WithNameColon(null));
        }

        // Find containing member (method, property, constructor, etc.) and containing type
        var containingMember = invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>(
            n => n is MethodDeclarationSyntax or PropertyDeclarationSyntax or ConstructorDeclarationSyntax or EventDeclarationSyntax);
        if (containingMember is null) return document;

        var containingType = containingMember.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType is null) return document;

        // Determine if the containing member is static
        bool isStaticContext = containingMember.Modifiers.Any(SyntaxKind.StaticKeyword);

        // Build constructor arguments: pattern [, options] — preserve original expressions
        var constructorArgs = new[] { SyntaxFactory.Argument(patternArg.Expression) }
            .Concat(constructorExtraArgs.Select(a => a));

        // Build field modifiers: private [static] readonly
        var modifiers = isStaticContext
            ? SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.StaticKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword))
            : SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

        // Create field: private [static] readonly Regex _regex = new Regex(pattern);
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
            .WithModifiers(modifiers)
            .NormalizeWhitespace()
            .WithLeadingTrivia(containingMember.GetLeadingTrivia())
            .WithTrailingTrivia(eol, eol);

        // Replace invocation: _regex.IsMatch(input [, remaining]) — preserve original input arg
        var instanceArgs = new[] { inputArg.WithNameColon(null) }
            .Concat(instanceExtraArgs);

        var newInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("_regex"),
                SyntaxFactory.IdentifierName(methodName)),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(instanceArgs)));

        editor.ReplaceNode(invocation, newInvocation);
        editor.InsertBefore(containingMember, fieldDeclaration);

        return editor.GetChangedDocument();
    }
}
