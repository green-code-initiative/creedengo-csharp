namespace Creedengo.Core.Analyzers;

/// <summary>GCI2508: Remove useless ToString() calls.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RemoveUselessToStringCall : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<OperationKind> Invocation = ImmutableArray.Create(OperationKind.Invocation);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI2508_RemoveUselessToStringCall,
        title: "Remove useless ToString() calls",
        message: "A ToString() call is unnecessary",

        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Useless ToString() calls should be removed to improve performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(static context => AnalyzeInvocation(context), Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var operation = (IInvocationOperation)context.Operation;
        if (operation.IsImplicit)
            return;

        // only parameterless ToString()
        if (!string.Equals(operation.TargetMethod.Name, nameof(object.ToString), StringComparison.Ordinal) || operation.TargetMethod.Parameters.Length != 0)
            return;

        if (operation.TargetMethod.ContainingType.SpecialType != SpecialType.System_String)
            return;

        // don't change null-conditional semantics: skip conditional access
        IOperation? ancestor = operation;
        while (ancestor != null)
        {
            if (ancestor is IConditionalAccessOperation)
                return;
            ancestor = ancestor.Parent;
        }

        ancestor = operation.Parent;
        while (ancestor != null && !(ancestor is IExpressionStatementOperation))
            ancestor = ancestor.Parent;

        if (ancestor is IExpressionStatementOperation)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, operation.Syntax.GetLocation()));
        }
    }
}
