namespace Creedengo.Core.Extensions;

/// <summary>Extensions methods for <see cref="ISymbol"/>.</summary>
internal static class SymbolExtensions
{
    /// <summary>Returns whether the symbol is a variable.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>True if the symbol is a variable, false otherwise.</returns>
    public static bool IsVariable(this ISymbol symbol) => symbol switch
    {
        IFieldSymbol fieldSymbol => !fieldSymbol.HasConstantValue,
        ILocalSymbol localSymbol => !localSymbol.HasConstantValue,
        _ => symbol is IPropertySymbol or IParameterSymbol,
    };

    /// <summary>Returns whether the symbol is a variable of the specified type.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="type">The variable type.</param>
    /// <returns>True if the symbol is a variable of the given type, false otherwise.</returns>
    public static bool IsVariableOfType(this ISymbol symbol, SpecialType type) => symbol switch
    {
        IFieldSymbol s => !s.HasConstantValue && s.Type.SpecialType == type,
        ILocalSymbol s => !s.HasConstantValue && s.Type.SpecialType == type,
        IPropertySymbol s => s.Type.SpecialType == type,
        IParameterSymbol s => s.Type.SpecialType == type,
        _ => false,
    };

    /// <summary>Returns whether the symbol implements a given interface.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="interfaceSymbol">The interface symbol.</param>
    /// <returns>True if the symbol implements the interface, false otherwise.</returns>
    public static bool ImplementsInterface(this ISymbol symbol, INamedTypeSymbol interfaceSymbol) =>
        SymbolEqualityComparer.Default.Equals(symbol.ContainingType, interfaceSymbol) ||
        symbol.ContainingType.AllInterfaces.Contains(interfaceSymbol, SymbolEqualityComparer.Default);

    /// <summary>Returns whether the symbol is declared outside the given loop.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <param name="loopNode">The loop node.</param>
    /// <returns>True if the symbol is declared outside the given loop, false otherwise.</returns>
    public static bool IsDeclaredOutsideLoop(this ISymbol symbol, SyntaxNode loopNode)
    {
        if (symbol is IParameterSymbol) // Parameters are always declared outside loops
            return true;

        if (symbol.FindDeclaringNode() is SyntaxNode declaringNode)
        {
            for (var node = loopNode.Parent; node is not null; node = node.Parent)
            {
                if (node == declaringNode)
                    return true;
            }
        }
        return false;
    }

    /// <summary>Returns the syntax node at which the symbol is declared.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>The declaring node, null if not found.</returns>
    public static SyntaxNode? FindDeclaringNode(this ISymbol symbol)
    {
        switch (symbol)
        {
            case ILocalSymbol localSymbol:
                var localRefs = localSymbol.DeclaringSyntaxReferences;
                if (localRefs.Length != 0)
                {
                    for (var node = localRefs[0].GetSyntax(); node is not null; node = node.Parent)
                    {
                        if (node is BlockSyntax or MethodDeclarationSyntax or CompilationUnitSyntax)
                            return node;
                    }
                }
                break;

            case IFieldSymbol fieldSymbol:
                var fieldRefs = fieldSymbol.ContainingType.DeclaringSyntaxReferences;
                if (fieldRefs.Length != 0) return fieldRefs[0].GetSyntax();
                break;

            case IPropertySymbol propertySymbol:
                var propRefs = propertySymbol.ContainingType.DeclaringSyntaxReferences;
                if (propRefs.Length != 0) return propRefs[0].GetSyntax();
                break;

            case IParameterSymbol parameterSymbol:
                var paramRefs = parameterSymbol.DeclaringSyntaxReferences;
                if (paramRefs.Length != 0) return paramRefs[0].GetSyntax();
                break;
        }
        return null;
    }

    /// <summary>Returns the visibility of a symbol.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>The visibility of the symbol.</returns>
    public static SymbolVisibility GetResultantVisibility(this ISymbol symbol)
    {
        if (symbol.Kind is SymbolKind.Alias or SymbolKind.TypeParameter)
            return SymbolVisibility.Private;

        if (symbol.Kind is SymbolKind.Parameter) // Parameters are only as visible as their containing symbol
            return symbol.ContainingSymbol.GetResultantVisibility();

        var visibility = SymbolVisibility.Public;
        while (symbol is not null && symbol.Kind is not SymbolKind.Namespace)
        {
            if (symbol.DeclaredAccessibility is Accessibility.NotApplicable or Accessibility.Private)
                return SymbolVisibility.Private;

            if (symbol.DeclaredAccessibility is Accessibility.Internal or Accessibility.ProtectedAndInternal)
                visibility = SymbolVisibility.Internal;

            symbol = symbol.ContainingSymbol;
        }
        return visibility;
    }

    /// <summary>Returns the overridden symbol of the given symbol.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>The overridden symbol, null if not found.</returns>
    public static ISymbol? OverriddenSymbol(this ISymbol symbol) =>
        !symbol.IsOverride ? null : symbol switch
        {
            IMethodSymbol s => s.OverriddenMethod,
            IPropertySymbol s => s.OverriddenProperty,
            IEventSymbol eventSymbol => eventSymbol.OverriddenEvent,
            _ => null,
        };

    internal static SyntaxNode GetSyntax(this ISymbol symbol, CancellationToken cancellationToken = default) => symbol
            .DeclaringSyntaxReferences[0]
            .GetSyntax(cancellationToken);
}
