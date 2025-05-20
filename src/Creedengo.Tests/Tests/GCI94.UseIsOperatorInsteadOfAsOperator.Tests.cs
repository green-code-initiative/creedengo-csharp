using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                if ([|x as string|] != null)
                {
        
                }
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
                if (null != [|x as string|])
                {
        
                }
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
