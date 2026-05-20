namespace Creedengo.Core.Analyzers;

/// <summary>GCI6605: Use List.Exists instead of LINQ Any with a predicate.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseExistsInsteadOfAny : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = ImmutableArray.Create(SyntaxKind.InvocationExpression);

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI6605_UseExistsInsteadOfAny,
        title: "Use Exists instead of Any",
        message: "Use 'Exists' instead of 'Any' for improved performance on List<T>",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "List<T>.Exists(predicate) is more performant than the LINQ extension method Enumerable.Any(predicate) for List<T> because it avoids enumerator allocation and interface dispatch overhead.");

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
            var enumerableType = startContext.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");
            if (enumerableType is null) return;

            var listType = startContext.Compilation.GetTypeByMetadataName("System.Collections.Generic.List`1");
            var expressionType = startContext.Compilation.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");

            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, enumerableType, listType, expressionType),
                SyntaxKinds);
        });
    }

    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol enumerableType,
        INamedTypeSymbol? listType,
        INamedTypeSymbol? expressionType)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;

        // Must be a member access like source.Any(predicate)
        if (invocation.Expression is not MemberAccessExpressionSyntax { Name.Identifier.Text: "Any" } memberAccess)
            return;

        // Resolve the method symbol — must be System.Linq.Enumerable.Any with the predicate parameter
        if (context.SemanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method ||
            !method.IsExtensionMethod ||
            method.Parameters.Length != 1 || // Reduced extension method: only the predicate parameter (excludes parameterless Any())
            !SymbolEqualityComparer.Default.Equals(method.ContainingType, enumerableType))
        {
            return;
        }

        // Receiver type must be List<T> or derive from it
        var receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression).Type;
        if (!IsList(receiverType, listType))
            return;

        // Exclude Expression<TDelegate> contexts (e.g. Entity Framework, NoSQL drivers)
        if (IsInsideExpressionTree(context.SemanticModel, invocation, expressionType))
            return;

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, memberAccess.Name.GetLocation()));
    }

    private static bool IsList(ITypeSymbol? type, INamedTypeSymbol? listType)
    {
        if (type is null || listType is null) return false;

        var current = type;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current.OriginalDefinition, listType))
                return true;
            current = current.BaseType;
        }
        return false;
    }

    private static bool IsInsideExpressionTree(SemanticModel semanticModel, SyntaxNode node, INamedTypeSymbol? expressionType)
    {
        if (expressionType is null) return false;

        for (var current = node.Parent; current is not null; current = current.Parent)
        {
            if (current is not LambdaExpressionSyntax and not AnonymousMethodExpressionSyntax)
                continue;

            var typeInfo = semanticModel.GetTypeInfo(current);
            var convertedType = typeInfo.ConvertedType;
            if (convertedType is INamedTypeSymbol namedType &&
                SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, expressionType))
            {
                return true;
            }
        }
        return false;
    }
}
