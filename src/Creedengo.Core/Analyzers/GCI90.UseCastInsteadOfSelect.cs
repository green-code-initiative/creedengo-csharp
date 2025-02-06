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
                if (SymbolEqualityComparer.Default.Equals(type, enumerableType) || SymbolEqualityComparer.Default.Equals(type, queryableType))
                    AnalyzeUseCastInsteadOfSelect(context, operation);
            }, OperationKind.Invocation);
        });
    }

    private static void AnalyzeUseCastInsteadOfSelect(OperationAnalysisContext context, IInvocationOperation operation)
    {
        // If what's returned is not a cast value or the cast is done by 'as' operator
        if (operation.Arguments[1].Descendants().OfType<IReturnOperation>().FirstOrDefault() is not { } returnOp ||
            returnOp.ReturnedValue is not IConversionOperation castOp || castOp.IsTryCast || castOp.Type is null)
        {
            return;
        }

        // Ensure the code is valid after replacement. The semantic may be different if you use Cast<T>() instead of Select(x => (T)x)
        // Handle enums: source.Select<MyEnum, byte>(item => (byte)item);
        // Using Cast<T> is only possible when the enum underlying type is the same as the conversion type
        if (castOp.Operand.Kind is not OperationKind.ParameterReference || // If the cast is not applied directly to the source element
            castOp.Conversion.IsUserDefined || castOp.Conversion.IsNumeric ||
            GetActualType(castOp.Operand) is INamedTypeSymbol { EnumUnderlyingType: { } enumerationType } &&
            !SymbolEqualityComparer.Default.Equals(enumerationType, castOp.Type))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor,
            operation.Syntax.ChildNodes().FirstOrDefault() is MemberAccessExpressionSyntax memberAccessExpression
            ? memberAccessExpression.Name.GetLocation()
            : operation.Syntax.GetLocation()));

        static ITypeSymbol? GetActualType(IOperation operation) =>
            operation is IConversionOperation conversionOperation
            ? GetActualType(conversionOperation.Operand)
            : operation.Type;
    }
}
