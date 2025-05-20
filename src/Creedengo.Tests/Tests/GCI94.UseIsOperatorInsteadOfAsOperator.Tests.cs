namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseIsOperatorInsteadOfAsOperatorTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UseIsOperatorInsteadOfAsOperator>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ([|x as string|] != null){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsReversedAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (null != [|x as string|]){ }
            }
        }
        """
        );


    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithLogicalExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if ([|x as string|] != null && booleen) { }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithLogicalExpressionRevertAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (null != [|x as string|] && booleen){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithInversedLogicalExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (booleen || [|x as string|] != null){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithMultipleLogicalExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleenA = x.Length > 0;
                var booleenB = x.Length > 0;
                if (booleenA || [|x as string|] != null && booleenB){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithMultipleAsExpressionAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var y = new TestClass();
                var booleen = x.Length > 0;
                if (booleen || [|x as string|] != null && [|y as TestClass|] != null){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithNegativeConditionalAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (!((x as string) == null)) { }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotWarnWhenIsOperatorIsUsedAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (x is string)
                {
        
                }
            }
        }
        """
        );

}
