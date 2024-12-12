namespace Creedengo.Extensions;

/// <summary>Extensions methods for <see cref="INamedTypeSymbol"/>.</summary>
public static class NamedTypeSymbolExtensions
{
    /// <summary>Returns whether the symbol is externally public, ie declared public and contained in public types.</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>True if the symbol is externally public, false otherwise.</returns>
    public static bool IsExternallyPublic(this INamedTypeSymbol symbol)
    {
        do
        {
            if (symbol.DeclaredAccessibility is not Accessibility.Public) return false;
            symbol = symbol.ContainingType;
        } while (symbol is not null);
        return true;
    }

    /// <summary>Returns whether a symbol has any overridable member, excluding the default ones (Equals, GetHashCode, etc).</summary>
    /// <param name="symbol">The symbol.</param>
    /// <returns>True if any member is overridable, false otherwise.</returns>
    public static bool HasAnyExternallyOverridableMember(this INamedTypeSymbol symbol)
    {
        if (symbol.TypeKind is not TypeKind.Class || symbol.IsSealed)
            return false;

        var (current, sealedMembers) = (symbol, default(HashSet<ISymbol>));
        do
        {
            if (current.SpecialType is SpecialType.System_Object) break;

            foreach (var member in current.GetMembers())
            {
                if (member.IsImplicitlyDeclared || member.DeclaredAccessibility is not Accessibility.Public and not Accessibility.Protected)
                    continue; // IsImplicitlyDeclared is for record methods, ie. Equals, GetHashCode, etc

                if (member.IsVirtual && sealedMembers?.Contains(member) != true) return true;

                // If overridden, skip base object methods, ie. Equals, GetHashCode, etc
                if (member.OverriddenSymbol() is { } overridden && overridden.ContainingType.SpecialType is not SpecialType.System_Object)
                {
                    if (member.IsSealed) // Cache the overriden member to prevent returning true when checking it in the parent
                        _ = (sealedMembers ??= new(SymbolEqualityComparer.Default)).Add(overridden);
                    else if (sealedMembers?.Contains(overridden) != true)
                        return true; // Overriden but not sealed is still overridable
                }
            }
            current = current.BaseType;
        } while (current is not null);

        return false;
    }
}
