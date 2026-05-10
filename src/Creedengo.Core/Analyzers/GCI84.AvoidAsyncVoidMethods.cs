namespace Creedengo.Core.Analyzers;

/// <summary>GCI84: Avoid async void methods.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AvoidAsyncVoidMethods : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> Declarations = ImmutableArray.Create(SyntaxKind.MethodDeclaration, SyntaxKind.LocalFunctionStatement);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI84_AvoidAsyncVoidMethods,
        title: "Avoid async void methods",
        message: "Avoid async void methods",
        category: Rule.Categories.Design,
        severity: DiagnosticSeverity.Warning,
        description: "Avoid async void methods, return a Task instead.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeMethod(context), Declarations);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var (modifiers, returnType, identifier) = context.Node switch
        {
            MethodDeclarationSyntax m => (m.Modifiers, m.ReturnType, m.Identifier),
            LocalFunctionStatementSyntax l => (l.Modifiers, l.ReturnType, l.Identifier),
            _ => default
        };

        if (modifiers.Any(SyntaxKind.AsyncKeyword) &&
            returnType is PredefinedTypeSyntax predefined &&
            predefined.Keyword.IsKind(SyntaxKind.VoidKeyword))
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, identifier.GetLocation()));
        }
    }
}
