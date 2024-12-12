namespace Creedengo.Tests;

[TestClass]
public sealed class UseWhereBeforeOrderByTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseWhereBeforeOrderBy, UseWhereBeforeOrderByFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    #region Method syntax

    [TestMethod]
    public Task DontWarnOnWhereOnlyMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .Where(x => x > 10)
                    .Select(x => x);
            }            
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByOnlyMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .OrderBy(x => x)
                    .Select(x => x);
            }            
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByDescendingOnlyMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .OrderByDescending(x => x)
                    .Select(x => x);
            }            
        }
        """);

    [TestMethod]
    public Task DontWarnOnRightOrderMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .Select(x => x);
            }            
        }
        """);

    [TestMethod]
    public Task WarnOnWrongOrderMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.OrderBy(x => x).[|Where|](x => x > 10).Select(x => x);
                var query2 = items
                    .OrderBy(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.Where(x => x > 10).OrderBy(x => x).Select(x => x);
                var query2 = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .Select(x => x);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnWrongOrderDescendingMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.OrderByDescending(x => x).[|Where|](x => x > 10).Select(x => x);
                var query2 = items
                    .OrderByDescending(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.Where(x => x > 10).OrderByDescending(x => x).Select(x => x);
                var query2 = items
                    .Where(x => x > 10)
                    .OrderByDescending(x => x)
                    .Select(x => x);
            }
        }
        """);

    [TestMethod]
    public Task WarnOnWrongMultipleOrderMethodAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.OrderBy(x => x).ThenByDescending(x => x).ThenBy(x => x).[|Where|](x => x > 10).Select(x => x);
                var query2 = items
                    .OrderBy(x => x)
                    .ThenByDescending(x => x)
                    .ThenBy(x => x)
                    .[|Where|](x => x > 10)
                    .Select(x => x);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query1 = items.Where(x => x > 10).OrderBy(x => x).ThenByDescending(x => x).ThenBy(x => x).Select(x => x);
                var query2 = items
                    .Where(x => x > 10)
                    .OrderBy(x => x)
                    .ThenByDescending(x => x)
                    .ThenBy(x => x)
                    .Select(x => x);
            }
        }
        """);

    #endregion

    #region Query syntax

    [TestMethod]
    public Task DontWarnOnWhereOnlyQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            select item;
            }            
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByOnlyQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item
                            select item;
            }            
        }
        """);

    [TestMethod]
    public Task DontWarnOnOrderByDescendingOnlyQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item descending
                            select item;
            }            
        }
        """);

    [TestMethod]
    public Task DontWarnOnRightOrderQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            orderby item
                            select item;
            }
        }
        """);

    [TestMethod]
    public Task WarnOnWrongOrderQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item
                            [|where|] item > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            orderby item
                            select item;
            }
        }
        """);

    [TestMethod]
    public Task WarnOnWrongMultipleOrderQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query = from item in items
                            orderby item.A
                            orderby item.B
                            [|where|] item.A > 10
                            select item;
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
                var query = from item in items
                            where item.A > 10
                            orderby item.A
                            orderby item.B
                            select item;
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnDisjointedOrderQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item
                            select item
                            into item
                            where item > 0
                            select item;
            }
        }
        """);

    [TestMethod]
    public Task WarnOnWrongOrderDescendingQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item descending
                            [|where|] item > 10
                            select item;
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            where item > 10
                            orderby item descending
                            select item;
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnDisjointedOrderDescendingQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<int>();
                var query = from item in items
                            orderby item descending
                            select item
                            into item
                            where item > 0
                            select item;
            }
        }
        """);

    [TestMethod]
    public Task WarnOnWrongMultipleOrderDescendingQueryAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static void Run()
            {
                var items = new List<(int A, int B)>();
                var query = from item in items
                            orderby item.A descending
                            orderby item.B descending
                            [|where|] item.A > 10
                            select item;
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
                var query = from item in items
                            where item.A > 10
                            orderby item.A descending
                            orderby item.B descending
                            select item;
            }
        }
        """);

    #endregion
}
