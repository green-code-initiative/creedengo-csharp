namespace Creedengo.Sandbox.Tests;

// Mirrors GCI95.UseIsOperatorInsteadOfAsOperator.Tests.cs.
// 'x as T' expressions marked "warns" should light up GCI95 in the IDE; "ok" lines should stay clean.
internal class GCI95Sandbox
{
    public void Positive_IfNotNull()
    {
        var x = "Hello";
        if (x as string != null) { } // warns
    }

    public void Positive_IfNotNullReversed()
    {
        var x = "Hello";
        if (null != x as string) { } // warns
    }

    public void Positive_RedundantParens()
    {
        var x = "Hello";
        if ((((null != x as string)))) { } // warns
    }

    public void Positive_WhileLoop()
    {
        var x = "Hello";
        while (x as string != null) { } // warns
    }

    public bool Positive_ReturnExpression(string x) => x as string != null; // warns

    public void Positive_DoWhileLoop()
    {
        var x = "Hello";
        do { } while (x as string != null); // warns
    }

    public void Positive_ForLoop()
    {
        var x = "Hello";
        for (; x as string != null;) { } // warns
    }

    public void Positive_TypeCheckOnMethodResult()
    {
        if (Helper() as SubType != null) { } // warns
    }

    public void Positive_TernaryCondition()
    {
        var x = "Hello";
        var result = x as string != null ? x : "World"; // warns
    }

    public void Negative_TernaryNotNullComparison()
    {
        var x = "Hello";
        var result = x as string != "toto" ? x : "World"; // ok — comparing to a string literal, not null
    }

    public void Positive_LogicalExpression()
    {
        var x = "Hello";
        var b = x.Length > 0;
        if (x as string != null && b) { } // warns
    }

    public void Positive_LogicalExpressionInverse()
    {
        var x = "Hello";
        var b = x.Length > 0;
        if (b || x as string != null) { } // warns
    }

    public void Positive_MultipleAsExpressions()
    {
        var x = "Hello";
        var y = new GCI95Sandbox();
        var b = x.Length > 0;
        if (b || x as string != null && y as GCI95Sandbox != null) { } // warns twice
    }

    public void Negative_NegativeConditional()
    {
        var x = "Hello";
        if (!((x as string) == null)) { } // ok — analyzer doesn't unwrap negation around equality
    }

    public void Negative_AlreadyIs()
    {
        var x = "Hello";
        if (x is string) { } // ok
    }

    public void Positive_EqualsNull()
    {
        var x = "Hello";
        if (x as string == null) return; // warns — analyzer rewrites both == null and != null
    }

    public void Negative_NullConditionalAccess()
    {
        var x = "Hello";
        if ((x as string)?.Length != 0) { } // ok — comparison is on the chained Length, not the 'as' result
    }

    private SubType Helper() => new();
    public class SubType : GCI95Sandbox;
}
