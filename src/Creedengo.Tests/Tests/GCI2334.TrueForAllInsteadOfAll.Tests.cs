namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class TrueForAllInsteadOfAllTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<TrueForAllInsteadOfAll, TrueForAllInsteadOfAllFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    // --- No-diagnostic cases ---

    [TestMethod] // .All() on an array — source type is not List<T>
    public Task AllOnArrayNoDiagnosticAsync() => VerifyAsync("""
        using System.Linq;

        public class Test
        {
            public void Run()
            {
                int[] arr = { 1, 2, 3 };
                bool result = arr.All(x => x > 0);
            }
        }
        """);

    [TestMethod] // .All() on IEnumerable<T> — source type is not List<T>
    public Task AllOnIEnumerableNoDiagnosticAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public void Run(IEnumerable<int> values)
            {
                bool result = values.All(x => x > 0);
            }
        }
        """);

    [TestMethod] // .All() on IList<T> — source type is not List<T>
    public Task AllOnIListNoDiagnosticAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public void Run(IList<int> values)
            {
                bool result = values.All(x => x > 0);
            }
        }
        """);

    [TestMethod] // .TrueForAll() already used — no LINQ call to flag
    public Task TrueForAllAlreadyUsedNoDiagnosticAsync() => VerifyAsync("""
        using System.Collections.Generic;

        public class Test
        {
            public void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                bool result = list.TrueForAll(x => x > 0);
            }
        }
        """);

    [TestMethod] // custom type with an All method — not from Enumerable
    public Task AllOnCustomTypeWithAllMethodNoDiagnosticAsync() => VerifyAsync("""
        public class MyCollection
        {
            public bool All(System.Func<int, bool> predicate) => true;
        }

        public class Test
        {
            public void Run()
            {
                var col = new MyCollection();
                bool result = col.All(x => x > 0);
            }
        }
        """);

    // --- Positive cases (diagnostic + fix) ---

    [TestMethod]
    public Task AllOnListWithLambdaShouldUseTrueForAllAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                bool result = list.[|All|](x => x > 0);
            }
        }
        """,
        """
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                bool result = list.TrueForAll(x => x > 0);
            }
        }
        """);

    [TestMethod]
    public Task AllOnListWithMethodGroupShouldUseTrueForAllAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            private static bool IsPositive(int x) => x > 0;

            public void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                bool result = list.[|All|](IsPositive);
            }
        }
        """,
        """
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            private static bool IsPositive(int x) => x > 0;

            public void Run()
            {
                var list = new List<int> { 1, 2, 3 };
                bool result = list.TrueForAll(IsPositive);
            }
        }
        """);

    [TestMethod]
    public Task AllOnListParameterShouldUseTrueForAllAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public bool Run(List<string> items)
            {
                return items.[|All|](s => s.Length > 0);
            }
        }
        """,
        """
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public bool Run(List<string> items)
            {
                return items.TrueForAll(s => s.Length > 0);
            }
        }
        """);

    [TestMethod]
    public Task AllOnMultipleListsShouldUseTrueForAllAsync() => VerifyAsync("""
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public void Run()
            {
                var list1 = new List<int> { 1, 2, 3 };
                var list2 = new List<string> { "a", "b" };
                bool a = list1.[|All|](x => x > 0);
                bool b = list2.[|All|](s => s.Length > 0);
            }
        }
        """,
        """
        using System.Collections.Generic;
        using System.Linq;

        public class Test
        {
            public void Run()
            {
                var list1 = new List<int> { 1, 2, 3 };
                var list2 = new List<string> { "a", "b" };
                bool a = list1.TrueForAll(x => x > 0);
                bool b = list2.TrueForAll(s => s.Length > 0);
            }
        }
        """);
}
