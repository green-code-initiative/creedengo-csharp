namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseIsOperatorInsteadOfAsOperatorTests
{
    private static readonly CodeFixerDlg VerifyAndFixAsync = TestRunner.VerifyAsync<UseIsOperatorInsteadOfAsOperator, UseIsOperatorInsteadOfAsOperatorFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAndFixAsync("");

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ([|x as string|] != null){ }
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (x is string){ }
            }
        }
        """);

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsReversedAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (null != [|x as string|]){ }
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (x is string){ }
            }
        }
        """);

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsReversedUselessParenthesisAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ((((null != [|x as string|])))){ }
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ((((x is string)))){ }
            }
        }
        """);


    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWhileLoopAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                while ([|x as string|] != null){ }
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                while (x is string){ }
            }
        }
        """);


    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsReturnAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            bool TestMethod(string x)
            {
                return [|x as string|] != null;
            }
        }
        """,
        """
        class TestClass
        {
            bool TestMethod(string x)
            {
                return x is string;
            }
        }
        """);

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsReturnWithoutAsAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            string TestMethod(string x)
            {
                return "Hello";
            }
        }
        """,
        """
        class TestClass
        {
            string TestMethod(string x)
            {
                return "Hello";
            }
        }
        """);

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsDoWhileLoopAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                do {}
                while ([|x as string|] != null);
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                do {}
                while (x is string);
            }
        }
        """);


    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsForLoopAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                for (;[|x as string|] != null;){ }
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                for (;x is string;){ }
            }
        }
        """);

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsTypeCheckAsync() => VerifyAndFixAsync("""
        class SubTestClass : TestClass { }

        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ([|TestMethod2() as SubTestClass|] != null){ }
            }

            TestClass TestMethod2()
            {
                return new SubTestClass();
            }
        }
        """,
        """
        class SubTestClass : TestClass { }

        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (TestMethod2() is SubTestClass){ }
            }

            TestClass TestMethod2()
            {
                return new SubTestClass();
            }
        }
        """
    );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsTernaryOperationAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
               var x = "Hello";
               var result = [|x as string|] != null ? x : "World";
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
               var x = "Hello";
               var result = x is string ? x : "World";
            }
        }
        """);

        [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsTernaryOperationNotNullComparisonAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
               var x = "Hello";
               var result = x as string != "toto" ? x : "World";
            }
        }
        """,
        """
        class TestClass
        {
            void TestMethod()
            {
               var x = "Hello";
               var result = x as string != "toto" ? x : "World";
            }
        }
        """);


    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithLogicalExpressionAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if ([|x as string|] != null && booleen) { }
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (x is string && booleen) { }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithLogicalExpressionRevertAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (null != [|x as string|] && booleen){ }
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (x is string && booleen){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithInversedLogicalExpressionAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (booleen || [|x as string|] != null){ }
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleen = x.Length > 0;
                if (booleen || x is string){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithMultipleLogicalExpressionAsync() => VerifyAndFixAsync("""
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
        """, """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var booleenA = x.Length > 0;
                var booleenB = x.Length > 0;
                if (booleenA || x is string && booleenB){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithMultipleAsExpressionAsync() => VerifyAndFixAsync("""
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
        """, """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                var y = new TestClass();
                var booleen = x.Length > 0;
                if (booleen || x is string && y is TestClass){ }
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotUseAsOperatorInsteadOfIsWithNegativeConditionalAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (!((x as string) == null)) { }
            }
        }
        """, """
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
    public Task DoNotWarnWhenIsOperatorIsUsedAsync() => VerifyAndFixAsync("""
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
        """, """
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

    [TestMethod]
    public Task DoNotWarmWhenAsOperatorIsWhenOperatorIsDifferentOfNotEqualsAsync() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ([|x as string|] == null) return;
            }
        }
        """, """
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (x is string) return;
            }
        }
        """
        );

    [TestMethod]
    public Task DoNotWarmWhenUsingAsWithNullConditionalAccess() => VerifyAndFixAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if ((x as string)?.Length != 0){ }
            }
        }
        """
        );

}
