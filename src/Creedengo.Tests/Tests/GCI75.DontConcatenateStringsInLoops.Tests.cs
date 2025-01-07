namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class DontConcatenateStringsInLoopsTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<DontConcatenateStringsInLoops>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    private const string WarningCode = """

        {
            string si = i.ToString();
            [|s += si|];
            [|s = s + si|];
            [|s = si + s|];
            s = si + si;
        }
        """;

    #region Regular loops

    [TestMethod]
    public Task DontConcatenateStringsInLoopsParameterWithAllLoopsAsync() => VerifyAsync($$"""
        using System.Linq;
        class Test
        {
            void Run1(string s)
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }

            void Run2(string s)
            {
                foreach (int i in Enumerable.Range(0, 10)){{WarningCode}}
            }

            void Run3(string s)
            {
                int i = 0;
                while (i++ < 10){{WarningCode}}
            }

            void Run4(string s)
            {
                int i = 0;
                do{{WarningCode}} while (++i < 10);
            }
        }
        """);

    [TestMethod]
    public Task DontConcatenateStringsInLoopsFieldAsync() => VerifyAsync($$"""
        class Test
        {
            string s = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """);

    [TestMethod]
    public Task DontConcatenateStringsInLoopsPropertyAsync() => VerifyAsync($$"""
        class Test
        {
            string s { get; set; } = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """);

    [TestMethod]
    public Task DontConcatenateStringsInLoopsStaticFieldAsync() => VerifyAsync($$"""
        class Test
        {
            static string s = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """);

    [TestMethod]
    public Task DontConcatenateStringsInLoopsStaticPropertyAsync() => VerifyAsync($$"""
        class Test
        {
            static string s { get; set; } = string.Empty;
            void Run()
            {
                for (int i = 0; i < 10; i++){{WarningCode}}
            }
        }
        """);

    [TestMethod]
    public Task DontConcatenateStringsInLoopsLocalVariableAsync() => VerifyAsync($$"""
        class Test
        {
            void Run()
            {
                string s = string.Empty;
                for (int i = 0; i < 10; i++){{WarningCode}}

                for (int i = 0; i < 10; i++)
                {
                    string si = i.ToString();
                    string s2 = string.Empty;
                    s2 += si;
                    s2 = s2 + si;
                    s2 = si + s2;
                    s2 = si + si;
                }
            }
        }
        """);

    #endregion

    #region ForEach methods

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("ImmutableList<int>")]
    public Task DontConcatenateStringsInForEachParameterAsync(string type) => VerifyAsync($$"""
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        public class Test
        {
            public void Run({{type}} list, string s)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());

                Parallel.ForEach(list, i =>{{WarningCode}});
                Parallel.ForEach(list, i => [|s += i.ToString()|]);
                Parallel.ForEach(list, i => [|s = s + i.ToString()|]);
                Parallel.ForEach(list, i => [|s = i.ToString() + s|]);
                Parallel.ForEach(list, i => s = i.ToString() + i.ToString());
            }
        }
        """);

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("ImmutableList<int>")]
    public Task DontConcatenateStringsInForEachFieldAsync(string type) => VerifyAsync($$"""
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        public class Test
        {
            private string s = string.Empty;
            public void Run({{type}} list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
        
                Parallel.ForEach(list, i =>{{WarningCode}});
                Parallel.ForEach(list, i => [|s += i.ToString()|]);
                Parallel.ForEach(list, i => [|s = s + i.ToString()|]);
                Parallel.ForEach(list, i => [|s = i.ToString() + s|]);
                Parallel.ForEach(list, i => s = i.ToString() + i.ToString());
            }
        }
        """);

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("ImmutableList<int>")]
    public Task DontConcatenateStringsInForEachPropertyAsync(string type) => VerifyAsync($$"""
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        public class Test
        {
            private string s { get; set; } = string.Empty;
            public void Run({{type}} list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
        
                Parallel.ForEach(list, i =>{{WarningCode}});
                Parallel.ForEach(list, i => [|s += i.ToString()|]);
                Parallel.ForEach(list, i => [|s = s + i.ToString()|]);
                Parallel.ForEach(list, i => [|s = i.ToString() + s|]);
                Parallel.ForEach(list, i => s = i.ToString() + i.ToString());
            }
        }
        """);

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("ImmutableList<int>")]
    public Task DontConcatenateStringsInForEachStaticFieldAsync(string type) => VerifyAsync($$"""
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        public class Test
        {
            private static string s = string.Empty;
            public void Run({{type}} list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
        
                Parallel.ForEach(list, i =>{{WarningCode}});
                Parallel.ForEach(list, i => [|s += i.ToString()|]);
                Parallel.ForEach(list, i => [|s = s + i.ToString()|]);
                Parallel.ForEach(list, i => [|s = i.ToString() + s|]);
                Parallel.ForEach(list, i => s = i.ToString() + i.ToString());
            }
        }
        """);

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("ImmutableList<int>")]
    public Task DontConcatenateStringsInForEachStaticPropertyAsync(string type) => VerifyAsync($$"""
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        public class Test
        {
            private static string s { get; set; } = string.Empty;
            public void Run({{type}} list)
            {
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());
        
                Parallel.ForEach(list, i =>{{WarningCode}});
                Parallel.ForEach(list, i => [|s += i.ToString()|]);
                Parallel.ForEach(list, i => [|s = s + i.ToString()|]);
                Parallel.ForEach(list, i => [|s = i.ToString() + s|]);
                Parallel.ForEach(list, i => s = i.ToString() + i.ToString());
            }
        }
        """);

    [TestMethod]
    [DataRow("List<int>")]
    [DataRow("ImmutableList<int>")]
    public Task DontConcatenateStringsInForEachLocalVariableAsync(string type) => VerifyAsync($$"""
        using System.Collections.Generic;
        using System.Collections.Immutable;
        using System.Threading.Tasks;
        public class Test
        {
            public void Run({{type}} list)
            {
                string s = string.Empty;
                list.ForEach(i =>{{WarningCode}});
                list.ForEach(i => [|s += i.ToString()|]);
                list.ForEach(i => [|s = s + i.ToString()|]);
                list.ForEach(i => [|s = i.ToString() + s|]);
                list.ForEach(i => s = i.ToString() + i.ToString());

                Parallel.ForEach(list, i =>{{WarningCode}});
                Parallel.ForEach(list, i => [|s += i.ToString()|]);
                Parallel.ForEach(list, i => [|s = s + i.ToString()|]);
                Parallel.ForEach(list, i => [|s = i.ToString() + s|]);
                Parallel.ForEach(list, i => s = i.ToString() + i.ToString());

                list.ForEach(i =>
                {
                    string si = i.ToString();
                    string s2 = string.Empty;
                    s2 += si;
                    s2 = s2 + si;
                    s2 = si + s2;
                    s2 = si + si;
                });

                Parallel.ForEach(list, i =>
                {
                    string si = i.ToString();
                    string s2 = string.Empty;
                    s2 += si;
                    s2 = s2 + si;
                    s2 = si + s2;
                    s2 = si + si;
                });
            }
        }
        """);

    #endregion
}
