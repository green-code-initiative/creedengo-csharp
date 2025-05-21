namespace Creedengo.Core.Analyzers;

/// <summary>GCI96: Use 'EventArgs.Empty' instead of 'new EventArgs()'.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseEventArgsDotEmpty : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI96_UseEventArgsDotEmpty,
        title: "Use 'EventArgs.Empty' instead of 'new EventArgs()'",
        message: "'new EventArgs()' is used instead of 'EventArgs.Empty' to avoid unnecessary allocations.",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Use 'EventArgs.Empty' instead of creating a new instance of 'EventArgs' to improve performance and reduce memory allocations.");


    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ObjectCreationExpression);
    }

    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreation)
            return;

        if (context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol is not INamedTypeSymbol typeSymbol)
            return;

        if (typeSymbol.ToDisplayString() != "System.EventArgs")
            return;

        if (objectCreation.ArgumentList?.Arguments.Any() != false)
            return;

        var diagnostic = Diagnostic.Create(Descriptor, objectCreation.GetLocation());
        context.ReportDiagnostic(diagnostic);
    }
}
