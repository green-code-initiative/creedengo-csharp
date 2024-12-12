using System.Collections.Concurrent;

namespace Creedengo.Analyzers;

/// <summary>GCI85: Make type sealed.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MakeTypeSealed : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SymbolKind> SymbolKinds = [SymbolKind.NamedType];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI85_MakeTypeSealed,
        title: "Make type sealed",
        message: "Type may be sealed, as it has no subtypes in its assembly and no user-declared overridable member",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Info,
        description: "When a type has no subtypes within its assembly and no user-declared overridable member, it may be sealed, which can improve performance.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            // Concurrent collections as RegisterSymbolAction is called once per symbol and can be parallelized
            var sealableClasses = new ConcurrentBag<INamedTypeSymbol>();
            var inheritedClasses = new ConcurrentDictionary<INamedTypeSymbol, bool>(SymbolEqualityComparer.Default);

            compilationStartContext.RegisterSymbolAction(analysisContext =>
            {
                if (analysisContext.Symbol is not INamedTypeSymbol symbol || symbol.TypeKind is not TypeKind.Class || symbol.IsStatic)
                    return;

                if (symbol.BaseType is INamedTypeSymbol { IsAbstract: false, SpecialType: SpecialType.None, TypeKind: TypeKind.Class } baseType)
                    _ = inheritedClasses.TryAdd(baseType, true);

                if (symbol.IsAbstract || symbol.IsSealed || symbol.IsScriptClass || symbol.IsImplicitlyDeclared || symbol.IsImplicitClass)
                    return;

                // Exclude types that are externally public (ie. inheritable from another assembly)
                // AND have at least one externally public or protected overridable member
                // An externally public type can still be inherited from without the second condition
                // But in that case we'll let the user decide whether to seal it or not, and mute the warning if so
                if (!symbol.IsExternallyPublic() || !symbol.HasAnyExternallyOverridableMember())
                    sealableClasses.Add(symbol);
            }, SymbolKinds);

            compilationStartContext.RegisterCompilationEndAction(compilationEndContext =>
            {
                foreach (var cls in sealableClasses)
                {
                    if (!inheritedClasses.ContainsKey(cls))
                        compilationEndContext.ReportDiagnostic(Diagnostic.Create(Descriptor, cls.Locations[0]));
                }
            });
        });
    }
}
