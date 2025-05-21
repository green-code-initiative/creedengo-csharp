namespace Creedengo.Core.Analyzers;

/// <summary>GCI82 fixer: Variable can be made constant.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(VariableCanBeMadeConstantFixer)), Shared]
public sealed class VariableCanBeMadeConstantFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [VariableCanBeMadeConstant.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        if (context.Diagnostics.Length == 0) return;

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
            var node = token.Parent?.AncestorsAndSelf().FirstOrDefault(n => n is LocalDeclarationStatementSyntax || n is FieldDeclarationSyntax);
            if (node is null) continue;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Make variable constant",
                    createChangedDocument: ct => RefactorAsync(context.Document, node, ct),
                    equivalenceKey: "Make variable constant"),
                diagnostic);
        }
    }

    private static async Task<Document> RefactorAsync(Document document, SyntaxNode node, CancellationToken token)
    {
        switch (node)
        {
            case LocalDeclarationStatementSyntax localDecl:
                return await RefactorLocalAsync(document, localDecl, token).ConfigureAwait(false);
            case FieldDeclarationSyntax fieldDecl:
                return await RefactorFieldAsync(document, fieldDecl, token).ConfigureAwait(false);
            default:
                return document;
        }
    }

    private static async Task<Document> RefactorLocalAsync(Document document, LocalDeclarationStatementSyntax localDecl, CancellationToken token)
    {
        // Remove the leading trivia from the local declaration.
        var firstToken = localDecl.GetFirstToken();
        var leadingTrivia = firstToken.LeadingTrivia;
        var trimmedLocal = leadingTrivia.Any()
            ? localDecl.ReplaceToken(firstToken, firstToken.WithLeadingTrivia(SyntaxTriviaList.Empty))
            : localDecl;

        // Create a const token with the leading trivia.
        var constToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));

        // If the type of the declaration is 'var', create a new type name for the inferred type.
        var varDecl = localDecl.Declaration;
        var varTypeName = varDecl.Type;
        if (varTypeName.IsVar)
            varDecl = await GetDeclarationForVarAsync(document, varDecl, varTypeName, token).ConfigureAwait(false);

        // Produce the new local declaration with an annotation
        var formattedLocal = trimmedLocal
            .WithModifiers(trimmedLocal.Modifiers.Insert(0, constToken)) // Insert the const token into the modifiers list
            .WithDeclaration(varDecl)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        if (root is null) return document;
        var newRoot = root.ReplaceNode(localDecl, formattedLocal);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> RefactorFieldAsync(Document document, FieldDeclarationSyntax fieldDecl, CancellationToken token)
    {
        // Remove static and readonly, add const after access modifiers
        var modifiers = fieldDecl.Modifiers;
        var accessModifiers = modifiers.Where(m =>
            m.IsKind(SyntaxKind.PublicKeyword) ||
            m.IsKind(SyntaxKind.PrivateKeyword) ||
            m.IsKind(SyntaxKind.ProtectedKeyword) ||
            m.IsKind(SyntaxKind.InternalKeyword)).ToList();
        var otherModifiers = modifiers.Where(m =>
            !m.IsKind(SyntaxKind.PublicKeyword) &&
            !m.IsKind(SyntaxKind.PrivateKeyword) &&
            !m.IsKind(SyntaxKind.ProtectedKeyword) &&
            !m.IsKind(SyntaxKind.InternalKeyword) &&
            !m.IsKind(SyntaxKind.StaticKeyword) &&
            !m.IsKind(SyntaxKind.ReadOnlyKeyword)).ToList();

        var constToken = SyntaxFactory.Token(fieldDecl.GetLeadingTrivia(), SyntaxKind.ConstKeyword, SyntaxFactory.TriviaList(SyntaxFactory.ElasticMarker));
        var newModifiers = new SyntaxTokenList();
        newModifiers = newModifiers.AddRange(accessModifiers);
        newModifiers = newModifiers.Add(constToken);
        newModifiers = newModifiers.AddRange(otherModifiers);

        // If the type is 'var', replace with the actual type
        var declaration = fieldDecl.Declaration;
        var typeSyntax = declaration.Type;
        if (typeSyntax.IsVar)
        {
            var semanticModel = await document.GetSemanticModelAsync(token).ConfigureAwait(false);
            var type = semanticModel?.GetTypeInfo(typeSyntax, token).ConvertedType;
            if (type != null && type.Name != "var")
            {
                declaration = declaration.WithType(
                    SyntaxFactory.ParseTypeName(type.ToDisplayString())
                        .WithLeadingTrivia(typeSyntax.GetLeadingTrivia())
                        .WithTrailingTrivia(typeSyntax.GetTrailingTrivia())
                        .WithAdditionalAnnotations(Simplifier.Annotation));
            }
        }

        var newField = fieldDecl
            .WithModifiers(newModifiers)
            .WithDeclaration(declaration)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var root = await document.GetSyntaxRootAsync(token).ConfigureAwait(false);
        if (root is null) return document;
        var newRoot = root.ReplaceNode(fieldDecl, newField);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<VariableDeclarationSyntax> GetDeclarationForVarAsync(Document document, VariableDeclarationSyntax varDecl, TypeSyntax varTypeName, CancellationToken token)
    {
        var semanticModel = await document.GetSemanticModelAsync(token).ConfigureAwait(false);

        if (semanticModel is null || semanticModel.GetAliasInfo(varTypeName, token) is not null)
            return varDecl; // Special case: Ensure that 'var' isn't actually an alias to another type (e.g. using var = System.String)

        var type = semanticModel.GetTypeInfo(varTypeName, token).ConvertedType;
        if (type is null || type.Name == "var") return varDecl; // Special case: Ensure that 'var' isn't actually a type named 'var'

        // Create a new TypeSyntax for the inferred type. Be careful to keep any leading and trailing trivia from the var keyword.
        return varDecl.WithType(SyntaxFactory
            .ParseTypeName(type.ToDisplayString())
            .WithLeadingTrivia(varTypeName.GetLeadingTrivia())
            .WithTrailingTrivia(varTypeName.GetTrailingTrivia())
            .WithAdditionalAnnotations(Simplifier.Annotation));
    }
}
