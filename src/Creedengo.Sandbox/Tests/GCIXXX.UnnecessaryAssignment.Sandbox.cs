using System;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCIXXX.UnnecessaryAssignment.Tests.cs.
// Blocks marked "warns" should light up GCIXXX in the IDE; "ok" methods should stay clean.
internal static class GCIXXXSandbox
{
    public static int Negative_GoodIfStatement()
    {
        bool f = false;
        if (f) return 2;
        else if (f) return 3;
        return 1; // ok — variable 'x' isn't created just to be reassigned in branches
    }

    public static int Negative_SimpleIfStatement()
    {
        bool f = false;
        int x = 1;
        if (f) x = 2; // ok — only one branch reassigns; the initial value is meaningful
        return x;
    }

    public static int Positive_IfStatement()
    {
        bool f = false;
        int x = 1;
        if (f) // warns — every branch reassigns x, so the initial '= 1' is unnecessary
        {
            x = 2;
        }
        else if (f)
        {
            x = 3;
        }
        return x;
    }

    public static void Negative_IfStatementNoReturn()
    {
        bool f = false;
        int x = 1;
        if (f) x = 2;
        else if (f) x = 3;
        x = 4; // ok — x is reassigned again after the if, so initial value is dead anyway (different rule)
    }

    public static int Positive_IfStatementThrow()
    {
        bool f = false;
        int x = 1;
        if (f) // warns — both branches assign, the 'else throw' makes the chain exhaustive
        {
            x = 2;
        }
        else if (f)
        {
            x = 3;
        }
        else
        {
            throw new InvalidOperationException();
        }
        return x;
    }

    public static int Negative_GoodSwitchStatement()
    {
        string? s = null;
        switch (s)
        {
            case "a": return 2;
            case "b": return 3;
        }
        return 1; // ok — no temp variable
    }

    public static int Positive_SwitchStatement()
    {
        string? s = null;
        int x = 1;
        switch (s) // warns — every case reassigns x
        {
            case "a": x = 2; break;
            case "b": x = 3; break;
        }
        return x;
    }

    public static void Negative_SwitchStatementNoReturn()
    {
        string? s = null;
        int x = 1;
        switch (s)
        {
            case "a": x = 2; break;
            case "b": x = 3; break;
        }
        x = 4; // ok — reassigned after switch
    }

    public static int Positive_SwitchStatementThrow()
    {
        string? s = null;
        int x = 1;
        switch (s) // warns
        {
            case "a": x = 2; break;
            case "b": x = 3; break;
            default: throw new InvalidOperationException();
        }
        return x;
    }

    public static void Negative_PolymorphicIf()
    {
        var fun = (bool flag) =>
        {
            object x;
            if (flag) x = new A(); // ok — branches assign different runtime types
            else x = new B();
            return x;
        };
        _ = fun(true);
    }

    public static void Negative_PolymorphicSwitch()
    {
        var fun = (object o) =>
        {
            object x;
            switch (o)
            {
                case int: x = new A(); break; // ok — polymorphic
                default: x = new B(); break;
            }
            return x;
        };
        _ = fun(0);
    }

    public static void Negative_AssignmentToMethodParameter(int p, bool f)
    {
        if (f) p = 1; // ok — assigning to a parameter, not a local
        else p = 2;
        _ = p;
    }

    private sealed class A;
    private sealed class B;
}
