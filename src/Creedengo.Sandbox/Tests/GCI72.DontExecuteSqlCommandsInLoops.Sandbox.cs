using System.Collections.Generic;
using System.Data;

namespace Creedengo.Sandbox.Tests;

// Mirrors GCI72.DontExecuteSqlCommandsInLoops.Tests.cs.
// Lines marked "warns" should light up GCI72 in the IDE; "ok" lines should stay clean.
internal static class GCI72Sandbox
{
    public static void Negative_OutsideLoop()
    {
        var command = default(IDbCommand)!;
        _ = command.ExecuteNonQuery(); // ok — not in a loop
        _ = command.ExecuteScalar();
        _ = command.ExecuteReader();
        _ = command.ExecuteReader(CommandBehavior.Default);
    }

    public static void Positive_ForLoop()
    {
        var command = default(IDbCommand)!;
        for (int i = 0; i < 10; i++)
        {
            _ = command.ExecuteNonQuery(); // warns
            _ = command.ExecuteScalar(); // warns
            _ = command.ExecuteReader(); // warns
            _ = command.ExecuteReader(CommandBehavior.Default); // warns
        }
    }

    public static void Positive_ForeachLoop(IEnumerable<int> items)
    {
        var command = default(IDbCommand)!;
        foreach (var item in items)
        {
            _ = command.ExecuteNonQuery(); // warns
        }
    }

    public static void Positive_WhileLoop()
    {
        var command = default(IDbCommand)!;
        int i = 0;
        while (i < 10)
        {
            _ = command.ExecuteScalar(); // warns
            i++;
        }
    }

    public static void Positive_DoWhileLoop()
    {
        var command = default(IDbCommand)!;
        int i = 0;
        do
        {
            _ = command.ExecuteReader(); // warns
            i++;
        } while (i < 10);
    }
}
