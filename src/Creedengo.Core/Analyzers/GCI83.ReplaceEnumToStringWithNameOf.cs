namespace Creedengo.Core.Analyzers;

/// <summary>GCI83: Replace enum ToString with nameof.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceEnumToStringWithNameOf : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<OperationKind> Invocation = ImmutableArray.Create(OperationKind.Invocation);
    private static readonly ImmutableArray<OperationKind> Interpolation = ImmutableArray.Create(OperationKind.Interpolation);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI83_ReplaceEnumToStringWithNameOf,
        title: "Replace enum ToString with nameof",
        message: "Enum.ToString() can be replaced with nameof()",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Replace enum ToString with nameof to resolve the call at compile time instead of runtime.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = ImmutableArray.Create(Descriptor);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static startContext =>
        {
            // C#6 is the floor (nameof is a C#6 feature). Bail out at the compilation level.
            if (startContext.Compilation.GetLanguageVersion() < LanguageVersion.CSharp6)
                return;

            // Resolve the well-known symbols once. System.String / System.Enum are part of mscorlib;
            // any compilation that lacks them is in such bad shape that we shouldn't try to analyze it.
            var stringType = startContext.Compilation.GetSpecialType(SpecialType.System_String);
            var enumType = startContext.Compilation.GetSpecialType(SpecialType.System_Enum);
            var stringEmpty = startContext.Compilation.GetTypeByMetadataName("System.String")?.GetMembers("Empty").OfType<IFieldSymbol>().FirstOrDefault();

            startContext.RegisterOperationAction(
                opContext => AnalyzeInvocation(opContext, stringType, enumType, stringEmpty),
                Invocation);
            startContext.RegisterOperationAction(AnalyzeInterpolation, Interpolation);
        });
    }

    private static bool UsesConstantFormat(object? format) =>
        format is null || format is string str && (str.Length == 0 || str is "g" or "G" or "f" or "F");

    private static void AnalyzeInvocation(
        OperationAnalysisContext context,
        INamedTypeSymbol stringType,
        INamedTypeSymbol enumType,
        IFieldSymbol? stringEmpty)
    {
        if (context.Operation is not IInvocationOperation { TargetMethod.Name: nameof(object.ToString) } operation ||
            !SymbolEqualityComparer.Default.Equals(operation.TargetMethod.ContainingType, enumType) ||
            operation.Instance is not IMemberReferenceOperation { Member.ContainingType.EnumUnderlyingType: { } })
        {
            return;
        }

        if (operation.Arguments.Length > 1) return;
        if (operation.Arguments.Length == 1)
        {
            var value = operation.Arguments[0].Value;
            if (!SymbolEqualityComparer.Default.Equals(value.Type, stringType) ||
                value is not IConversionOperation { Operand.ConstantValue: { HasValue: true, Value: null } } &&
                (value is not ILiteralOperation { ConstantValue: { HasValue: true, Value: var format } } || !UsesConstantFormat(format)) &&
                (value is not IFieldReferenceOperation fieldRef || stringEmpty is null || !SymbolEqualityComparer.Default.Equals(fieldRef.Field, stringEmpty)))
            {
                return;
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, operation.Syntax.GetLocation()));
    }

    private static void AnalyzeInterpolation(OperationAnalysisContext context)
    {
        if (context.Operation is not IInterpolationOperation operation ||
            operation.Expression is not IMemberReferenceOperation { Member.ContainingType.EnumUnderlyingType: { } } ||
            operation.FormatString is ILiteralOperation { ConstantValue: { HasValue: true, Value: var value } } && !UsesConstantFormat(value))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, operation.Syntax.GetLocation()));
    }
}
