namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class ReplaceStaticReadonlyByConstTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<ReplaceStaticReadonlyByConst, ReplaceStaticReadonlyByConstFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontWarnOnConstAsync() => VerifyAsync("""
        public class TestClass
        {
            const int x = 1;
            const string s = "Bar";
        }
        """);

    [TestMethod]
    public Task WarnOnStaticReadonlyIntAsync() => VerifyAsync("""
        public class TestClass
        {
            [|static readonly int x = 1;|]
        }
        """, """
        public class TestClass
        {
            const int x = 1;
        }
        """);

    [TestMethod]
    public Task WarnOnStaticReadonlyStringAsync() => VerifyAsync("""
        public class TestClass
        {
            [|static readonly string s = "Bar";|]
        }
        """, """
        public class TestClass
        {
            const string s = "Bar";
        }
        """);

    [TestMethod]
    public Task DontWarnOnStaticReadonlyNonConstTypeAsync() => VerifyAsync("""
        public class TestClass
        {
            static readonly object o = new object();
        }
        """);

    [TestMethod]
    public Task DontWarnOnStaticReadonlyNonConstantValueAsync() => VerifyAsync("""
        public class TestClass
        {
            static readonly int x = System.Environment.TickCount;
        }
        """);

    [TestMethod]
    public Task WarnOnMultipleStaticReadonlyFieldsAsync() => VerifyAsync("""
        public class TestClass
        {
            [|static readonly int a = 1;|]
            [|static readonly string b = "foo";|]
        }
        """, """
        public class TestClass
        {
            const int a = 1;
            const string b = "foo";
        }
        """);

    [TestMethod]
    public Task DontWarnOnStaticReadonlyWithoutInitializerAsync() => VerifyAsync("""
        public class TestClass
        {
            static readonly int x;
        }
        """);
}
