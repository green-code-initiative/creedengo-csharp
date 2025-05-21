namespace Creedengo.Core.Analyzers;

/// <summary>GCIACV: Do not pass non-read-only struct by read-only reference.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NonReadOnlyStruct : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> MethodDeclarations = [SyntaxKind.MethodDeclaration];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCIACV_NonReadOnlyStruct,
        title: "Do not pass non-read-only struct by read-only reference",
        message: "Non-read-only struct should not be passed by read-only reference.",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Using 'in' or 'ref readonly' parameter modifier for non-readonly struct types can lead to defensive copies, causing performance degradation.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeSyntaxNode(context), MethodDeclarations);
    }
    
    private static void AnalyzeSyntaxNode(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        var semanticModel = context.SemanticModel;
        var compilation = context.Compilation;        // Analyze each parameter in the method declaration
        foreach (var parameter in methodDeclaration.ParameterList.Parameters)
        {
            // Check if the parameter has either 'in' keyword or 'ref readonly' modifiers
            bool isReadOnlyReference = parameter.Modifiers.Any(SyntaxKind.InKeyword) || 
                (parameter.Modifiers.Any(SyntaxKind.RefKeyword) && parameter.Modifiers.Any(SyntaxKind.ReadOnlyKeyword));
            
            if (isReadOnlyReference &&
                parameter.Type != null &&
                ModelExtensions.GetTypeInfo(semanticModel, parameter.Type).Type is { } parameterTypeSymbol &&
                parameterTypeSymbol.IsValueType &&
                parameterTypeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                // For nested struct types or any other struct, check if it's readonly
                if (!namedTypeSymbol.IsReadOnly)
                {
                    // Report diagnostic on the type identifier part of the parameter
                    var diagnosticLocation = parameter.Type.GetLocation();
                    if (diagnosticLocation == null)
                        diagnosticLocation = parameter.GetLocation();

                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            Descriptor, 
                            diagnosticLocation, 
                            parameter.Identifier.ValueText));
                }
            }
        }
    }
}
