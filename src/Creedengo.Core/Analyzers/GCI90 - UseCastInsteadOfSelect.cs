namespace Creedengo.Core.Analyzers;

/// <summary>GCI90: Use Cast instead of Select to cast.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseCastInsteadOfSelect : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI90_UseCastInsteadOfSelect,
        title: "Use Cast instead of Select",
        message: "Use Cast instead of Select to cast",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Use Cast instead of Select to cast for better performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(static context =>
        {
            var enumerableType = context.Compilation.GetBestTypeByMetadataName("System.Linq.Enumerable");
            var queryableType = context.Compilation.GetBestTypeByMetadataName("System.Linq.Queryable");
            if (enumerableType is null && queryableType is null) return;

            context.RegisterOperationAction(context =>
            {
                if (context.Operation is not IInvocationOperation { Arguments.Length: 2, TargetMethod.Name: nameof(Enumerable.Select) } operation)
                    return;

                var type = operation.TargetMethod.ContainingType;
                if (enumerableType is not null && SymbolEqualityComparer.Default.Equals(type, enumerableType) ||
                    queryableType is not null && SymbolEqualityComparer.Default.Equals(type, queryableType))
                {
                    AnalyzeUseCastInsteadOfSelect(context, operation);
                }
            }, OperationKind.Invocation);
        });
    }

    private static void AnalyzeUseCastInsteadOfSelect(OperationAnalysisContext context, IInvocationOperation operation)
    {
        var semanticModel = operation.SemanticModel!;

        var selectorArg = operation.Arguments[1];

        var returnOp = selectorArg.Descendants().OfType<IReturnOperation>().FirstOrDefault();
        if (returnOp is null) return;

        // If what's returned is not a cast value or the cast is done by 'as' operator
        if (returnOp.ReturnedValue is not IConversionOperation castOp || castOp.IsTryCast || castOp.Type is null)
            return;

        // If the cast is not applied directly to the source element (one of the selector's arguments)
        if (castOp.Operand.Kind != OperationKind.ParameterReference)
            return;

        // Ensure the code is valid after replacement. The semantic may be different if you use Cast<T>() instead of Select(x => (T)x).
        // Current conversion: (Type)value
        // Cast<T>() conversion: (Type)(object)value
        if (!CanReplaceByCast(castOp))
            return;

        // Determine if we're casting to a nullable type.
        var selectMethodSymbol = semanticModel.GetSymbolInfo(operation.Syntax, context.CancellationToken).Symbol as IMethodSymbol;
        var nullableFlowState = selectMethodSymbol?.TypeArgumentNullableAnnotations[1] == NullableAnnotation.Annotated ?
            NullableFlowState.MaybeNull :
            NullableFlowState.None;

        var typeSyntax = castOp.Syntax;
        if (typeSyntax is CastExpressionSyntax castSyntax)
        {
            typeSyntax = castSyntax.Type;
        }

        // Get the cast type's minimally qualified name, in the current context
        var properties = CreateProperties(OptimizeLinqUsageData.UseCastInsteadOfSelect);

        var castType = castOp.Type.ToMinimalDisplayString(semanticModel, nullableFlowState, operation.Syntax.SpanStart);
        context.ReportDiagnostic(OptimizeLinqUsageAnalyzer.UseCastInsteadOfSelect, properties, operation, DiagnosticInvocationReportOptions.ReportOnMember, castType);

        static bool CanReplaceByCast(IConversionOperation op)
        {
            if (op.Conversion.IsUserDefined || op.Conversion.IsNumeric)
                return false;

            // Handle enums: source.Select<MyEnum, byte>(item => (byte)item);
            // Using Cast<T> is only possible when the enum underlying type is the same as the conversion type
            var operandActualType = op.Operand.GetActualType();
            var enumerationType = operandActualType.GetEnumerationType();
            if (enumerationType is not null)
            {
                if (!enumerationType.IsEqualTo(op.Type))
                    return false;
            }

            return true;
        }
    }
}
