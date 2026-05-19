namespace Creedengo.Core.Analyzers;

/// <summary>GC2333: Remove redundant 'ToCharArray' call.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveRedundantToCharArrayCall : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = ImmutableArray.Create(
        SyntaxKind.ForEachStatement);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GC2333_RemoveRedundantToCharArrayCall,
        title: "Remove redundant 'ToCharArray' call",
        message: "The 'ToCharArray' call is redundant",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "The 'ToCharArray' call is redundant and can be removed.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeLoopNode(context), SyntaxKinds);
    }

    private static void AnalyzeLoopNode(SyntaxNodeAnalysisContext context)
    {
        var forEachStatement = (ForEachStatementSyntax)context.Node;

        if (forEachStatement.Expression is not InvocationExpressionSyntax invocation)
            return;

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
            return;

        if (memberAccess.Name.Identifier.Text != "ToCharArray")
            return;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(memberAccess);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        if (methodSymbol.ContainingType.SpecialType != SpecialType.System_String)
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.Name.GetLocation()));
    }
}