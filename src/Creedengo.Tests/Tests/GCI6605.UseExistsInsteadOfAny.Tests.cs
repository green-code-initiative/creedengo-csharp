namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseExistsInsteadOfAnyTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseExistsInsteadOfAny, UseExistsInsteadOfAnyFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    #region Positive cases (should trigger diagnostic + fix)

    [TestMethod]
    public Task AnyWithLambdaOnListAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static bool Run()
            {
                var list = new List<int> { 1, 2, 3 };
                return list.[|Any|](x => x > 1);
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static bool Run()
            {
                var list = new List<int> { 1, 2, 3 };
                return list.Exists(x => x > 1);
            }
        }
        """);

    [TestMethod]
    public Task AnyWithMethodGroupOnListAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static bool Run()
            {
                var list = new List<string> { "a", "b" };
                return list.[|Any|](string.IsNullOrEmpty);
            }

            private static bool IsValid(string s) => !string.IsNullOrEmpty(s);
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static bool Run()
            {
                var list = new List<string> { "a", "b" };
                return list.Exists(string.IsNullOrEmpty);
            }

            private static bool IsValid(string s) => !string.IsNullOrEmpty(s);
        }
        """);

    [TestMethod]
    public Task AnyOnDerivedListAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyList<T> : List<T>;

            public static bool Run()
            {
                var list = new MyList<int> { 1, 2, 3 };
                return list.[|Any|](x => x > 1);
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyList<T> : List<T>;

            public static bool Run()
            {
                var list = new MyList<int> { 1, 2, 3 };
                return list.Exists(x => x > 1);
            }
        }
        """);

    [TestMethod]
    public Task AnyInConditionAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                if (list.[|Any|](x => x == 2))
                    Console.WriteLine("found");
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                if (list.Exists(x => x == 2))
                    Console.WriteLine("found");
            }
        }
        """);

    #endregion

    #region Negative cases (should NOT trigger diagnostic)

    [TestMethod]
    public Task DontTriggerOnParameterlessAnyAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static bool Run()
            {
                var list = new List<int> { 1, 2, 3 };
                return list.Any();
            }
        }
        """);

    [TestMethod]
    public Task DontTriggerOnNonListTypesAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var enumerable = Enumerable.Range(0, 10);
                _ = enumerable.Any(x => x > 5);

                var set = new HashSet<int> { 1, 2, 3 };
                _ = set.Any(x => x > 1);

                var dic = new Dictionary<int, string> { { 1, "a" } };
                _ = dic.Any(kv => kv.Key > 0);
            }
        }
        """);

    [TestMethod]
    public Task DontTriggerInsideExpressionTreeAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        using System.Linq.Expressions;
        public static class Test
        {
            public static void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                Expression<Func<bool>> expr = () => list.Any(x => x > 1);
            }
        }
        """);

    [TestMethod]
    public Task DontTriggerOnCustomAnyMethodAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>
            {
                public bool Any(Func<T, bool> predicate) => true;
            }

            public static bool Run()
            {
                var coll = new MyCollection<int> { 1, 2, 3 };
                return coll.Any(x => x > 1);
            }
        }
        """);

    [TestMethod]
    public Task DontTriggerOnArrayAsync() => VerifyAsync("""
        using System;
        using System.Linq;
        public static class Test
        {
            public static bool Run()
            {
                var arr = new int[] { 1, 2, 3 };
                return arr.Any(x => x > 1);
            }
        }
        """);

    #endregion
}
