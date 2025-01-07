namespace Creedengo.Core.Analyzers;

/// <summary>GCI81: Specify struct layout.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SpecifyStructLayout : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SymbolKind> SymbolKinds = [SymbolKind.NamedType];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI81_UseStructLayout,
        title: "Use struct layout",
        message: "Use struct layout",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Struct layouts should be explicitly declared, to avoid any unnecessary overhead.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(static context => Analyze(context), SymbolKinds);
    }

    private static void Analyze(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol symbol || !symbol.IsValueType || symbol.EnumUnderlyingType is not null)
            return;

        var structLayoutAttributeType = context.Compilation.GetBestTypeByMetadataName("System.Runtime.InteropServices.StructLayoutAttribute");
        if (structLayoutAttributeType is null) return;

        foreach (var attr in symbol.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(structLayoutAttributeType, attr.AttributeClass))
                return;
        }

        int memberCount = 0;
        foreach (var member in symbol.GetMembers())
        {
            if (member is not IFieldSymbol fieldSymbol || fieldSymbol.IsConst || fieldSymbol.IsStatic)
                continue;

            if (fieldSymbol.Type.IsReferenceType) return; // A struct containing a reference type is always in auto layout
            memberCount++;
        }

        if (memberCount < 2) return;
        foreach (var location in symbol.Locations)
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, location));
    }
}
