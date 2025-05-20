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
}
