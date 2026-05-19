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

        var regexTypeSymbol = editor.SemanticModel.Compilation.GetTypeByMetadataName("System.Text.RegularExpressions.Regex");
        if (regexTypeSymbol is null) return document;

        var methodName = memberAccess.Name.Identifier.Text;
        var args = invocation.ArgumentList.Arguments;
        if (args.Count < 2) return document;

        // First arg is input, second is pattern for static Regex methods.
        var inputArg = args[0];
        var patternArg = args[1];

        // Only offer the fix when the pattern is a constant expression: anything else would land
        // in a field initializer that cannot reference locals/parameters and produce uncompilable code.
        var patternConstant = editor.SemanticModel.GetConstantValue(patternArg.Expression, token);
        if (!patternConstant.HasValue) return document;

        var containingType = invocation.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (containingType is null) return document;

        var containingMember = invocation.FirstAncestorOrSelf<MemberDeclarationSyntax>(
            n => n is MethodDeclarationSyntax or PropertyDeclarationSyntax or ConstructorDeclarationSyntax
              or EventDeclarationSyntax or IndexerDeclarationSyntax or OperatorDeclarationSyntax
              or ConversionOperatorDeclarationSyntax or DestructorDeclarationSyntax);
        if (containingMember is null) return document;

        // A static lambda / static local function inside an instance member still requires a static field.
        bool isStaticContext = IsStaticContext(invocation, containingMember);

        // Split remaining args between constructor (RegexOptions / TimeSpan) and instance call (others).
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

        string fieldName = GenerateUniqueFieldName(containingType);

        // Build a fully-qualified Regex type expression annotated so Simplifier reduces it to `Regex`
        // when a using is in scope and so an import is added otherwise. Each call returns a fresh node
        // (a syntax node cannot have two parents).
        SyntaxNode RegexTypeExpression() => editor.Generator
            .TypeExpression(regexTypeSymbol)
            .WithAdditionalAnnotations(Simplifier.AddImportsAnnotation);

        var constructorArgs = new SyntaxList<ArgumentSyntax>()
            .Add(SyntaxFactory.Argument(patternArg.Expression))
            .AddRange(constructorExtraArgs);

        var objectCreation = SyntaxFactory.ObjectCreationExpression(
            (TypeSyntax)RegexTypeExpression(),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(constructorArgs)),
            initializer: null);

        var modifiers = isStaticContext
            ? DeclarationModifiers.Static | DeclarationModifiers.ReadOnly
            : DeclarationModifiers.ReadOnly;

        var fieldDeclaration = ((FieldDeclarationSyntax)editor.Generator.FieldDeclaration(
                fieldName,
                RegexTypeExpression(),
                Accessibility.Private,
                modifiers,
                objectCreation))
            .WithAdditionalAnnotations(Formatter.Annotation);

        // Build the instance call: <fieldName>.<methodName>(input [, instanceExtras]).
        var instanceArgs = new SyntaxList<ArgumentSyntax>()
            .Add(inputArg.WithNameColon(null))
            .AddRange(instanceExtraArgs);

        var newInvocation = SyntaxFactory.InvocationExpression(
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName(fieldName),
                SyntaxFactory.IdentifierName(methodName)),
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(instanceArgs)))
            .WithTriviaFrom(invocation);

        editor.ReplaceNode(invocation, newInvocation);
        editor.InsertBefore(containingMember, fieldDeclaration);

        return editor.GetChangedDocument();
    }

    private static bool IsStaticContext(SyntaxNode invocation, MemberDeclarationSyntax containingMember)
    {
        foreach (var ancestor in invocation.Ancestors())
        {
            if (ancestor == containingMember) break;
            if (ancestor is LocalFunctionStatementSyntax lf && lf.Modifiers.Any(SyntaxKind.StaticKeyword)) return true;
            if (ancestor is AnonymousFunctionExpressionSyntax af && af.Modifiers.Any(SyntaxKind.StaticKeyword)) return true;
        }
        return containingMember.Modifiers.Any(SyntaxKind.StaticKeyword);
    }

    private static string GenerateUniqueFieldName(TypeDeclarationSyntax containingType)
    {
        var existing = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in containingType.Members)
        {
            switch (member)
            {
                case FieldDeclarationSyntax field:
                    foreach (var v in field.Declaration.Variables) existing.Add(v.Identifier.ValueText);
                    break;
                case EventFieldDeclarationSyntax eventField:
                    foreach (var v in eventField.Declaration.Variables) existing.Add(v.Identifier.ValueText);
                    break;
                case PropertyDeclarationSyntax prop: existing.Add(prop.Identifier.ValueText); break;
                case EventDeclarationSyntax ev: existing.Add(ev.Identifier.ValueText); break;
                case MethodDeclarationSyntax method: existing.Add(method.Identifier.ValueText); break;
            }
        }

        const string baseName = "_regex";
        if (!existing.Contains(baseName)) return baseName;
        for (int i = 1; ; i++)
        {
            var candidate = baseName + i;
            if (!existing.Contains(candidate)) return candidate;
        }
    }
}
