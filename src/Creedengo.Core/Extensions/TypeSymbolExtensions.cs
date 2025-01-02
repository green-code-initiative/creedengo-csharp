namespace Creedengo.Core.Extensions;

internal static class TypeSymbolExtensions
{
    /// <summary>Returns whether the type is a primitive number type.</summary>
    /// <param name="type">The type.</param>
    /// <returns>True if the type is a primitive number type, false otherwise.</returns>
    public static bool IsPrimitiveNumber(this ITypeSymbol type) => type.SpecialType is
        SpecialType.System_Int32 or
        SpecialType.System_Int64 or
        SpecialType.System_UInt32 or
        SpecialType.System_UInt64 or
        SpecialType.System_Int16 or
        SpecialType.System_UInt16 or
        SpecialType.System_Byte or
        SpecialType.System_SByte;
}
