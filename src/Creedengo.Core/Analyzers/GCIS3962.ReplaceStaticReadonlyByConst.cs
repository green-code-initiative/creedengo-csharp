namespace Creedengo.Core.Analyzers;

/// <summary>GCIS3962: Replace static readonly by const when possible.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReplaceStaticReadonlyByConst : DiagnosticAnalyzer
{
    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI3962_ReplaceStaticReadonlyByConst,
        title: "Replace static readonly by const",
        message: "Field '{0}' can be made const",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Static readonly fields initialized with compile-time constants should be declared as const for better performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static c => AnalyzeField(c), SyntaxKind.FieldDeclaration);
    }

    private static void AnalyzeField(SyntaxNodeAnalysisContext context)
    {
        var fieldDecl = (FieldDeclarationSyntax)context.Node;

        // Must be static readonly, not already const
        if (!fieldDecl.Modifiers.Any(SyntaxKind.StaticKeyword) ||
            !fieldDecl.Modifiers.Any(SyntaxKind.ReadOnlyKeyword) ||
            fieldDecl.Modifiers.Any(SyntaxKind.ConstKeyword))
            return;

        // Only one variable per declaration for simplicity
        foreach (var variable in fieldDecl.Declaration.Variables)
        {
            // Must have initializer
            if (variable.Initializer is null)
                continue;

            var variableSymbol = context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) as IFieldSymbol;
            if (variableSymbol is null)
                continue;

            // Only allow types that can be const
            if (!IsAllowedConstType(variableSymbol.Type))
                continue;

            // Must be assigned a compile-time constant
            var constantValue = context.SemanticModel.GetConstantValue(variable.Initializer.Value, context.CancellationToken);
            if (!constantValue.HasValue)
                continue;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, fieldDecl.GetLocation(), variable.Identifier.Text));
        }
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

