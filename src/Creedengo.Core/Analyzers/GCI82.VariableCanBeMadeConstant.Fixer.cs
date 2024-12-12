namespace Creedengo.Analyzers;

/// <summary>GCI82 dixer: Variable can be made constant.</summary>
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
            var parent = root.FindToken(diagnostic.Location.SourceSpan.Start).Parent;
            if (parent is null) return;

            foreach (var node in parent.AncestorsAndSelf())
            {
                if (node is not LocalDeclarationStatementSyntax declaration) continue;
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "Make variable constant",
                        createChangedDocument: token => RefactorAsync(context.Document, declaration, token),
                        equivalenceKey: "Make variable constant"),
                    diagnostic);
                break;
            }
        }
    }

    private static async Task<Document> RefactorAsync(Document document, LocalDeclarationStatementSyntax localDecl, CancellationToken token)
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

        return await document.WithUpdatedRoot(localDecl, formattedLocal).ConfigureAwait(false);

        static async Task<VariableDeclarationSyntax> GetDeclarationForVarAsync(Document document, VariableDeclarationSyntax varDecl, TypeSyntax varTypeName, CancellationToken token)
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
}
