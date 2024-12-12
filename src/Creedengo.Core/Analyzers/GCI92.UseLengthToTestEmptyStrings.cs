namespace Creedengo.Analyzers;

/// <summary>GCI92: Use Length to test empty strings.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseLengthToTestEmptyStrings : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> EqualsExpression = [SyntaxKind.EqualsExpression];
    private static readonly ImmutableArray<SyntaxKind> NotEqualsExpression = [SyntaxKind.NotEqualsExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI92_UseStringEmptyLength,
        title: "Use Length to test empty strings",
        message: "Use string.Length instead of comparison with empty string",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Warning,
        description: "Use string.Length instead of comparison with empty string for better readability and performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeComparison(context), EqualsExpression);
        context.RegisterSyntaxNodeAction(static context => AnalyzeComparison(context), NotEqualsExpression);
    }

    private static void AnalyzeComparison(SyntaxNodeAnalysisContext context)
    {
        var binaryExpression = (BinaryExpressionSyntax)context.Node;
        var (left, right) = (binaryExpression.Left, binaryExpression.Right);

        bool report = left.IsEmptyStringLiteral()
            ? right.SpecialType(context.SemanticModel) is SpecialType.System_String
            : right.IsEmptyStringLiteral() && left.SpecialType(context.SemanticModel) is SpecialType.System_String;

        if (report) context.ReportDiagnostic(Diagnostic.Create(Descriptor, binaryExpression.GetLocation()));
    }
}
