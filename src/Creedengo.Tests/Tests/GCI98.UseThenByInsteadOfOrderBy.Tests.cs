namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseThenByInsteadOfOrderByTests
{
    private static readonly CodeFixerDlg VerifyAsync =
        TestRunner.VerifyAsync<UseThenByInsteadOfOrderBy, UseThenByInsteadOfOrderByFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    #region No diagnostic

    [TestMethod]
    public Task DontWarnOnSingleOrderByAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderBy(x => x.A);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnSingleOrderByDescendingAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderByDescending(x => x.A);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByThenByAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderBy(x => x.A).ThenBy(x => x.B);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByDescendingThenByDescendingAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderByDescending(x => x.A).ThenByDescending(x => x.B);
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByFollowedBySelectAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderBy(x => x.A).Select(x => x.B);
            }
        }
        """);

    #endregion

    #region Diagnostic + fix

    [TestMethod]
    public Task WarnOnOrderByAfterOrderByAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query1 = items.OrderBy(x => x.A).[|OrderBy|](x => x.B);
                var query2 = items
                    .OrderBy(x => x.A)
                    .[|OrderBy|](x => x.B);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query1 = items.OrderBy(x => x.A).ThenBy(x => x.B);
                var query2 = items
                    .OrderBy(x => x.A)
                    .ThenBy(x => x.B);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnOrderByDescendingAfterOrderByAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderBy(x => x.A).[|OrderByDescending|](x => x.B);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderBy(x => x.A).ThenByDescending(x => x.B);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnOrderByAfterOrderByDescendingAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderByDescending(x => x.A).[|OrderBy|](x => x.B);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderByDescending(x => x.A).ThenBy(x => x.B);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnOrderByDescendingAfterOrderByDescendingAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderByDescending(x => x.A).[|OrderByDescending|](x => x.B);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var result = items.OrderByDescending(x => x.A).ThenByDescending(x => x.B);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnOrderByAfterThenByAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B, int C)>();
                var result = items.OrderBy(x => x.A).ThenBy(x => x.B).[|OrderBy|](x => x.C);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B, int C)>();
                var result = items.OrderBy(x => x.A).ThenBy(x => x.B).ThenBy(x => x.C);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnChainedOrderByAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B, int C)>();
                var result = items.OrderBy(x => x.A).[|OrderBy|](x => x.B).[|OrderBy|](x => x.C);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;

        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B, int C)>();
                var result = items.OrderBy(x => x.A).ThenBy(x => x.B).ThenBy(x => x.C);
            }
        }
        """);

    #endregion
}
