namespace Creedengo.Core.Analyzers;

/// <summary>GCI72: Don't execute SQL commands in loops.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontExecuteSqlCommandsInLoops : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> Invocations = [SyntaxKind.InvocationExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI72_DontExecuteSqlCommandsInLoops,
        title: "Don't execute SQL commands in loops",
        message: "A SQL command is executed in a loop",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "SQL commands should not be executed in loops, refactor them if possible.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            if (compilationStartContext.Compilation.GetTypeByMetadataName("System.Data.IDbCommand") is not INamedTypeSymbol iDbCommandTypeSymbol)
                return;

            compilationStartContext.RegisterSyntaxNodeAction(context =>
            {
                if (context.SemanticModel.GetSymbolInfo((InvocationExpressionSyntax)context.Node).Symbol is IMethodSymbol symbol &&
                    symbol.ImplementsInterface(iDbCommandTypeSymbol) &&
                    symbol.Name is "ExecuteNonQuery" or "ExecuteScalar" or "ExecuteReader" &&
                    context.Node.IsInsideLoop())
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
                }
            }, Invocations);
        });
    }
}
