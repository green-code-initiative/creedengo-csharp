namespace Creedengo.Extensions;

/// <summary>Extensions methods for <see cref="IOperation"/>.</summary>
public static class OperationExtensions
{
    /// <summary>Returns whether the given operation's target matches one of the given operations' target.</summary>
    /// <param name="op">The operation.</param>
    /// <param name="left">The left operation.</param>
    /// <param name="right">The right operation.</param>
    /// <returns>True if the operation's target matches one of the given operations' target, false otherwise.</returns>
    public static bool MatchesAnyOperand(this ISimpleAssignmentOperation op, IOperation left, IOperation right) => op.Target switch
    {
        IFieldReferenceOperation fieldOp =>
            left is IFieldReferenceOperation fieldLeft && SymbolEqualityComparer.Default.Equals(fieldOp.Field, fieldLeft.Field) ||
            right is IFieldReferenceOperation fieldRight && SymbolEqualityComparer.Default.Equals(fieldOp.Field, fieldRight.Field),
        IPropertyReferenceOperation propOp =>
            left is IPropertyReferenceOperation propLeft && SymbolEqualityComparer.Default.Equals(propOp.Property, propLeft.Property) ||
            right is IPropertyReferenceOperation propRight && SymbolEqualityComparer.Default.Equals(propOp.Property, propRight.Property),
        IParameterReferenceOperation paramOp =>
            left is IParameterReferenceOperation paramLeft && SymbolEqualityComparer.Default.Equals(paramOp.Parameter, paramLeft.Parameter) ||
            right is IParameterReferenceOperation paramRight && SymbolEqualityComparer.Default.Equals(paramOp.Parameter, paramRight.Parameter),
        ILocalReferenceOperation localOp =>
            left is ILocalReferenceOperation localLeft && SymbolEqualityComparer.Default.Equals(localOp.Local, localLeft.Local) ||
            right is ILocalReferenceOperation localRight && SymbolEqualityComparer.Default.Equals(localOp.Local, localRight.Local),
        _ => false,
    };
}
