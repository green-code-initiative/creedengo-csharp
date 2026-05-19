namespace Creedengo.Core.Analyzers;

/// <summary>GCI97: Use Regex instance instead of static method.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseRegexInstanceInsteadOfStaticMethod : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<OperationKind> Invocation = ImmutableArray.Create(OperationKind.Invocation);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI97_UseRegexInstanceInsteadOfStaticMethod,
        title: "Use Regex instance instead of static method",
        message: "Use a Regex instance instead of a static method to avoid repeated pattern parsing overhead",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Static Regex methods rely on a limited internal cache and incur pattern parsing overhead. Using an explicitly cached Regex instance makes caching deterministic and reduces CPU and energy consumption.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    private static readonly HashSet<string> StaticRegexMethods = new(StringComparer.Ordinal)
    {
        nameof(System.Text.RegularExpressions.Regex.IsMatch),
        nameof(System.Text.RegularExpressions.Regex.Match),
        nameof(System.Text.RegularExpressions.Regex.Matches),
        nameof(System.Text.RegularExpressions.Regex.Replace),
        nameof(System.Text.RegularExpressions.Regex.Split),
    };

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(static context => AnalyzeInvocation(context), Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation operation) return;

        var method = operation.TargetMethod;
        if (!method.IsStatic) return;

        var regexType = context.Compilation.GetTypeByMetadataName("System.Text.RegularExpressions.Regex");
        if (regexType is null) return;

        if (!SymbolEqualityComparer.Default.Equals(method.ContainingType, regexType)) return;
        if (!StaticRegexMethods.Contains(method.Name)) return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, operation.Syntax.GetLocation()));
    }
}
