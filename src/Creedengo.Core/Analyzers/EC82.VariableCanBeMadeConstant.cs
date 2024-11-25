namespace Creedengo.Core.Analyzers;

/// <summary>EC82: Variable can be made constant.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VariableCanBeMadeConstant : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [SyntaxKind.LocalDeclarationStatement];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC82_VariableCanBeMadeConstant,
        title: "Variable can be made constant",
        message: "A variable can be made constant",
        category: Rule.Categories.Usage,
        severity: DiagnosticSeverity.Info,
        description: "Variables that can be made constant should be, to resolve them at compile time instead of runtime.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeNode(context), SyntaxKinds);
    }

    private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        var localDeclaration = (LocalDeclarationStatementSyntax)context.Node;

        // Make sure the declaration isn't already const
        if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
            return;

        // Ensure that all variables in the local declaration have initializers that are assigned with constant values
        var variableType = context.SemanticModel.GetTypeInfo(localDeclaration.Declaration.Type, context.CancellationToken).ConvertedType;
        if (variableType is null) return;
        foreach (var variable in localDeclaration.Declaration.Variables)
        {
            var initializer = variable.Initializer;
            if (initializer is null) return;

            var constantValue = context.SemanticModel.GetConstantValue(initializer.Value, context.CancellationToken);
            if (!constantValue.HasValue) return;

            // Ensure that the initializer value can be converted to the type of the local declaration without a user-defined conversion.
            var conversion = context.SemanticModel.ClassifyConversion(initializer.Value, variableType);
            if (!conversion.Exists || conversion.IsUserDefined) return;

            // Special cases:
            // * If the constant value is a string, the type of the local declaration must be string
            // * If the constant value is null, the type of the local declaration must be a reference type
            if (constantValue.Value is string)
                if (variableType.SpecialType is not SpecialType.System_String) return;
            else if (variableType.IsReferenceType && constantValue.Value is not null)
            {
                return;
            }
        }

        // Perform data flow analysis on the local declaration
        var dataFlowAnalysis = context.SemanticModel.AnalyzeDataFlow(localDeclaration);
        if (dataFlowAnalysis is null) return;

        foreach (var variable in localDeclaration.Declaration.Variables)
        {
            // Retrieve the local symbol for each variable in the local declaration and ensure that it is not written outside of the data flow analysis region
            var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken);
            if (variableSymbol is null || dataFlowAnalysis.WrittenOutside.Contains(variableSymbol))
                return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, context.Node.GetLocation()));
    }
}
