
namespace Creedengo.Core.Analyzers;

/// <summary>GCI82: Variable can be made constant.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class VariableCanBeMadeConstant : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [SyntaxKind.LocalDeclarationStatement, SyntaxKind.FieldDeclaration];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI82_VariableCanBeMadeConstant,
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
        switch (context.Node)
        {
            case LocalDeclarationStatementSyntax localDeclaration:
                AnalyzeLocalDeclaration(context, localDeclaration);
                break;
            case FieldDeclarationSyntax fieldDeclaration:
                AnalyzeFieldDeclaration(context, fieldDeclaration);
                break;
        }
    }

    private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context, LocalDeclarationStatementSyntax localDeclaration)
    {
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
            {
                if (variableType.SpecialType is not SpecialType.System_String) return;
            }
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

    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context, FieldDeclarationSyntax fieldDecl)
    {
        // Ignore const fields
        if (fieldDecl.Modifiers.Any(SyntaxKind.ConstKeyword))
            return;

        // Only static readonly fields
        if (!fieldDecl.Modifiers.Any(SyntaxKind.StaticKeyword) ||
            !fieldDecl.Modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            return;

        var variableType = context.SemanticModel.GetTypeInfo(fieldDecl.Declaration.Type, context.CancellationToken).ConvertedType;
        if (variableType is null) return;

        foreach (var variable in fieldDecl.Declaration.Variables)
        {
            if (variable.Initializer is null) return;

            var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
            if (variableSymbol is null) return;

            // Only allow types that can be const
            if (!IsAllowedConstType(variableSymbol.Type)) return;

            var constantValue = context.SemanticModel.GetConstantValue(variable.Initializer.Value, context.CancellationToken);
            if (!constantValue.HasValue) return;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, fieldDecl.GetLocation()));
    }

    private static bool IsAllowedConstType(ITypeSymbol type)
    {
        switch (type.SpecialType)
        {
            case SpecialType.System_Boolean:
            case SpecialType.System_Byte:
            case SpecialType.System_SByte:
            case SpecialType.System_Char:
            case SpecialType.System_Decimal:
            case SpecialType.System_Double:
            case SpecialType.System_Single:
            case SpecialType.System_Int32:
            case SpecialType.System_UInt32:
            case SpecialType.System_Int64:
            case SpecialType.System_UInt64:
            case SpecialType.System_Int16:
            case SpecialType.System_UInt16:
            case SpecialType.System_String:
                return true;
            default:
                return false;
        }
    }
}
