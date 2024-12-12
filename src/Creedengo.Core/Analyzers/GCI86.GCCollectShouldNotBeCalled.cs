namespace Creedengo.Analyzers;

/// <summary>GCI86 : GC Collect should not be called.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class GCCollectShouldNotBeCalled : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [SyntaxKind.InvocationExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI86_GCCollectShouldNotBeCalled,
        title: "Avoid calling GC.Collect() method",
        message: "Avoid calling GC.Collect() method",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Avoid calling GC.Collect() method, as the cost often largely outweighs the benefits.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeMethod(context), SyntaxKinds);
    }

    private static void AnalyzeMethod(SyntaxNodeAnalysisContext context)
    {
        var invocationExpression = (InvocationExpressionSyntax)context.Node;

        if (context.SemanticModel.GetSymbolInfo(invocationExpression).Symbol is not IMethodSymbol methodSymbol ||
            methodSymbol.Name != nameof(GC.Collect) ||
            !SymbolEqualityComparer.Default.Equals(methodSymbol.ContainingType,
                context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(GC).FullName)))
        {
            return;
        }

        // If there is no arguments or the "generation" argument is not 0, report
        bool report = !invocationExpression.ArgumentList.Arguments.Any();
        if (!report)
        {
            var firstArgument = invocationExpression.ArgumentList.Arguments[0];
            if (firstArgument.NameColon is not null) // Named argument, may not be the one we want
            {
                string firstParameterName = methodSymbol.Parameters[0].Name; // Parameter name from the method signature
                if (firstArgument.NameColon.Name.Identifier.Text != firstParameterName)
                {
                    foreach (var argument in invocationExpression.ArgumentList.Arguments)
                    {
                        if (argument.NameColon?.Name.Identifier.Text == firstParameterName)
                        {
                            firstArgument = argument; // Should always be hit in this case
                            break;
                        }
                    }
                }
            }

            var constantValue = context.SemanticModel.GetConstantValue(firstArgument.Expression);
            report = constantValue.Value is not int intValue || intValue != 0;
        }

        if (report)
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation()));
    }
}
