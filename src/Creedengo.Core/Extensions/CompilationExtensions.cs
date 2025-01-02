namespace Creedengo.Core.Extensions;

/// <summary>Extension methods for <see cref="Compilation"/>.</summary>
internal static class CompilationExtensions
{
    /// <summary>Returns the language version of the compilation.</summary>
    /// <param name="compilation">The compilation.</param>
    /// <returns>The language version.</returns>
    public static LanguageVersion GetLanguageVersion(this Compilation compilation) =>
        ((CSharpCompilation)compilation).LanguageVersion;

    /// <summary>
    /// Gets a type by its metadata name to use for code analysis within a <see cref="Compilation"/>. This method
    /// attempts to find the "best" symbol to use for code analysis, which is the symbol matching the first of the
    /// following rules.
    ///
    /// <list type="number">
    ///   <item><description>
    ///     If only one type with the given name is found within the compilation and its referenced assemblies, that
    ///     type is returned regardless of accessibility.
    ///   </description></item>
    ///   <item><description>
    ///     If the current <paramref name="compilation"/> defines the symbol, that symbol is returned.
    ///   </description></item>
    ///   <item><description>
    ///     If exactly one referenced assembly defines the symbol in a manner that makes it visible to the current
    ///     <paramref name="compilation"/>, that symbol is returned.
    ///   </description></item>
    ///   <item><description>
    ///     Otherwise, this method returns <see langword="null"/>.
    ///   </description></item>
    /// </list>
    /// </summary>
    /// <param name="compilation">The <see cref="Compilation"/> to consider for analysis.</param>
    /// <param name="fullyQualifiedMetadataName">The fully-qualified metadata type name to find.</param>
    /// <returns>The symbol to use for code analysis; otherwise, <see langword="null"/>.</returns>
    /// <remarks>Comes from Roslynator sourc code :
    /// https://github.com/dotnet/roslyn/blob/d2ff1d83e8fde6165531ad83f0e5b1ae95908289/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/CompilationExtensions.cs#L11-L68
    /// </remarks>
    public static INamedTypeSymbol? GetBestTypeByMetadataName(this Compilation compilation, string fullyQualifiedMetadataName)
    {
        var type = default(INamedTypeSymbol);
        foreach (var currentType in compilation.GetTypesByMetadataName(fullyQualifiedMetadataName))
        {
            if (ReferenceEquals(currentType.ContainingAssembly, compilation.Assembly))
                return currentType;

            var visibility = currentType.GetResultantVisibility();
            if (visibility is SymbolVisibility.Public ||
                visibility is SymbolVisibility.Internal && currentType.ContainingAssembly.GivesAccessTo(compilation.Assembly))
            {
                if (type is object) return null; // Multiple visible types with the same metadata name are present
                type = currentType;
            }
        }
        return type;
    }
}
