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


    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static startContext =>
        {
            var eventArgsType = startContext.Compilation.GetTypeByMetadataName("System.EventArgs");
            if (eventArgsType is null) return;

            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeNode(nodeContext, eventArgsType),
                SyntaxKind.ObjectCreationExpression);
        });
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context, INamedTypeSymbol eventArgsType)
    {
        var objectCreation = (ObjectCreationExpressionSyntax)context.Node;

        // Argument list missing or has any args -> not "new EventArgs()" with empty parens. Bail out.
        if (objectCreation.ArgumentList is not { Arguments.Count: 0 })
            return;

        if (context.SemanticModel.GetSymbolInfo(objectCreation.Type).Symbol is not INamedTypeSymbol typeSymbol)
            return;

        if (!SymbolEqualityComparer.Default.Equals(typeSymbol, eventArgsType))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, objectCreation.GetLocation()));
    }
}
