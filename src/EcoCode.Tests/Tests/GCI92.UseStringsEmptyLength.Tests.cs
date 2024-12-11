namespace EcoCode.Tests;

[TestClass]
public sealed class UseStringEmptyLengthTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseLengthToTestEmptyStrings, UseLengthToTestEmptyStringsFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontUseStringEmptyLengthWithEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|test == ""|]) { }
            }
        }
        """);

    [TestMethod]
    public Task DontUseStringEmptyLengthReverseWithEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|"" == test|]) { }
            }
        }
        """);

    [TestMethod]
    public Task DontUseStringEmptyLengthWithNotEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|"" != test|]) { }
            }
        }
        """);

    [TestMethod]
    public Task UseStringEmptyLengthAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if (test.Length == 0) { }
            }
        }
        """);

    [TestMethod]
    public Task FixDontUseStringEmptyLengthWithEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|test == ""|]) { }
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if (test.Length == 0) { }
            }
        }
        """);

    [TestMethod]
    public Task FixDontUseStringEmptyLengthReverseWithEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|"" == test|]) { }
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if (test.Length == 0) { }
            }
        }
        """);

    [TestMethod]
    public Task FixDontUseStringEmptyLengthWithNotEqualsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if ([|test != ""|]) { }
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                string test = "test";
                if (test.Length != 0) { }
            }
        }
        """);
}
