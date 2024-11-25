namespace Creedengo.Core.Analyzers;

/// <summary>EC83: Replace enum ToString with nameof.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceEnumToStringWithNameOf : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<OperationKind> Invocation = [OperationKind.Invocation];
    private static readonly ImmutableArray<OperationKind> Interpolation = [OperationKind.Interpolation];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.EC83_ReplaceEnumToStringWithNameOf,
        title: "Replace enum ToString with nameof",
        message: "Enum.ToString() can be replaced with nameof()",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Replace enum ToString with nameof to resolve the call at compile time instead of runtime.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterOperationAction(static context => AnalyzeInvocation(context), Invocation);
        context.RegisterOperationAction(static context => AnalyzeInterpolation(context), Interpolation);
    }

    private static IFieldSymbol GetStringEmptySymbol(Compilation compilation) =>
        (IFieldSymbol)compilation.GetTypeByMetadataName("System.String")!.GetMembers("Empty")[0];

    private static bool UsesConstantFormat(object? format) =>
        format is null || format is string str && (str.Length == 0 || str is "g" or "G" or "f" or "F");

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        if (context.Compilation.GetLanguageVersion() < LanguageVersion.CSharp6 ||
            context.Operation is not IInvocationOperation { TargetMethod.Name: nameof(object.ToString) } operation ||
            !SymbolEqualityComparer.Default.Equals(operation.TargetMethod.ContainingType, context.Compilation.GetSpecialType(SpecialType.System_Enum)) ||
            operation.Instance is not IMemberReferenceOperation { Member.ContainingType.EnumUnderlyingType: { } })
        {
            return;
        }

        if (operation.Arguments.Length > 1) return;
        if (operation.Arguments.Length == 1)
        {
            var value = operation.Arguments[0].Value;
            if (!SymbolEqualityComparer.Default.Equals(value.Type, context.Compilation.GetSpecialType(SpecialType.System_String)) ||
                value is not IConversionOperation { Operand.ConstantValue: { HasValue: true, Value: null } } &&
                (value is not ILiteralOperation { ConstantValue: { HasValue: true, Value: var format } } || !UsesConstantFormat(format)) &&
                (value is not IFieldReferenceOperation fieldRef || !SymbolEqualityComparer.Default.Equals(fieldRef.Field, GetStringEmptySymbol(context.Compilation))))
            {
                return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, operation.Syntax.GetLocation()));
    }

    private static void AnalyzeInterpolation(OperationAnalysisContext context)
    {
        if (context.Compilation.GetLanguageVersion() < LanguageVersion.CSharp6 ||
            context.Operation is not IInterpolationOperation operation ||
            operation.Expression is not IMemberReferenceOperation { Member.ContainingType.EnumUnderlyingType: { } } ||
            operation.FormatString is ILiteralOperation { ConstantValue: { HasValue: true, Value: var value } } && !UsesConstantFormat(value))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, operation.Syntax.GetLocation()));
    }
}
