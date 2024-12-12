namespace Creedengo.Tests;

[TestClass]
public sealed class UseListIndexerTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseListIndexer, UseListIndexerFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    #region First

    [TestMethod]
    public Task RefactorFirstAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;

            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine([|arr.First|]());

                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine([|list.First|]());

                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine([|coll.First|]());
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;

            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine(arr[0]);
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine(list[0]);

                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll[0]);
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstWithNoIndexerAsync() => VerifyAsync("""
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var enumerable = Enumerable.Range(0, 10);
                return enumerable.First();
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstWithPredicateAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;
        
            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine(arr.First(x => x != 1));
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine(list.First(x => x != 1));
        
                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll.First(x => x != 1));
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstIfNotLinqAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>
            {
                public T First() => this[0];
            }

            public static void Run()
            {
                var myCollection = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(myCollection.First());
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorFirstNonListsAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var set = new HashSet<int> { 1, 2, 3 };
                Console.WriteLine(set.First());

                var dic = new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 3 } };
                Console.WriteLine(dic.First());
            }
        }
        """);

    #endregion

    #region Last

    [TestMethod]
    public Task RefactorLastWithIndexerAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;

            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine([|arr.Last|]());

                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine([|list.Last|]());

                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine([|coll.Last|]());
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;

            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine(arr[^1]);
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine(list[^1]);

                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll[^1]);
            }
        }
        """);

    [TestMethod]
    public Task RefactorLastWithCountOrLengthAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>
            {
            }

            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine([|arr.Last|]());

                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine([|list.Last|]());

                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine([|coll.Last|]());
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>
            {
            }

            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine(arr[arr.Length - 1]);
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine(list[list.Count - 1]);

                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll[coll.Count - 1]);
            }
        }
        """, languageVersion: LanguageVersion.CSharp7);

    [TestMethod]
    public Task DontRefactorLastWithNoIndexerAsync() => VerifyAsync("""
        using System.Linq;
        public static class Test
        {
            public static int GetFirstValue()
            {
                var enumerable = Enumerable.Range(0, 10);
                return enumerable.Last();
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorLastWithPredicateAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;
        
            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine(arr.Last(x => x != 1));
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine(list.Last(x => x != 1));
        
                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll.Last(x => x != 1));
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorLastIfNotLinqAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>
            {
                public T Last() => this[^1];
            }

            public static void Run()
            {
                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll.Last());
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorLastNonListsAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var set = new HashSet<int> { 1, 2, 3 };
                Console.WriteLine(set.Last());

                var dic = new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 3 } };
                Console.WriteLine(dic.Last());
            }
        }
        """);

    #endregion

    #region ElementAt

    [TestMethod]
    public Task UseIndexerInsteadOfElementAtAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;
        
            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine([|arr.ElementAt|](2));
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine([|list.ElementAt|](2));
        
                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine([|coll.ElementAt|](2));
            }
        }
        """, """
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>;
        
            public static void Run()
            {
                var arr = new int[] { 1, 2, 3 };
                Console.WriteLine(arr[2]);
        
                var list = new List<int> { 1, 2, 3 };
                Console.WriteLine(list[2]);
        
                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll[2]);
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorElementAtWithNoIndexerAsync() => VerifyAsync("""
        using System;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var enumerable = Enumerable.Range(0, 10);
                Console.WriteLine(enumerable.ElementAt(2));
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorElementAtIfNotLinqAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        public static class Test
        {
            private sealed class MyCollection<T> : List<T>
            {
                public T ElementAt(int index) => this[index];
            }

            public static void Run()
            {
                var coll = new MyCollection<int> { 1, 2, 3 };
                Console.WriteLine(coll.ElementAt(2));
            }
        }
        """);

    [TestMethod]
    public Task DontRefactorElementAtNonListsAsync() => VerifyAsync("""
        using System;
        using System.Collections.Generic;
        using System.Linq;
        public static class Test
        {
            public static void Run()
            {
                var set = new HashSet<int> { 1, 2, 3 };
                Console.WriteLine(set.ElementAt(2));

                var dic = new Dictionary<int, int> { { 1, 1 }, { 2, 2 }, { 3, 3 } };
                Console.WriteLine(dic.ElementAt(2));
            }
        }
        """);

    #endregion
}
