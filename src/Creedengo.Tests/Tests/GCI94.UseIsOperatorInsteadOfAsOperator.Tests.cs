using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseIsOperatorInsteadOfAsOperatorTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseIsOperatorInsteadOfAsOperator, UseIsOperatorInsteadOfAsOperatorFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task DontUseAsInsteadOfIsAsync() => VerifyAsync("""
        class TestClass
        {
            void TestMethod()
            {
                var x = "Hello";
                if (x as string != null)
                {
        
                }
            }
        }
        """,
        ///,
        //"""
        //class TestClass
        //{
        //    void TestMethod()
        //    {
        //        var x = "Hello";
        //        if (x is string)
        //        {
        
        //        }
        //    }
        //}
        //"""

        );

}
