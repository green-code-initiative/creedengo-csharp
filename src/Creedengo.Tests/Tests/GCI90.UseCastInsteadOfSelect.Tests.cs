namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseCastInsteadOfSelectTests
{
    private static readonly CodeFixerDlg VerifyAsync = TestRunner.VerifyAsync<UseCastInsteadOfSelect, UseCastInsteadOfSelectFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync("");

    [TestMethod]
    public Task WarnOnSimpleSelectAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static IEnumerable<object> Run(IEnumerable<string> values) => values.[|Select|](i => (object)i);
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static IEnumerable<object> Run(IEnumerable<string> values) => values.Cast<object>();
        }
        """);

    [TestMethod]
    public Task WarnOnSimpleSelectWithNullableAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static IEnumerable<object?> Run(IEnumerable<string?> values) => values.[|Select|](i => (object?)i);
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public static IEnumerable<object?> Run(IEnumerable<string?> values) => values.Cast<object?>();
        }
        """);

    [TestMethod]
    public Task WarnOnMultipleSelectAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public class BaseType { }
            public class DerivedType : BaseType { }

            public static void Run(IEnumerable<DerivedType> derived)
            {
                _ = derived.[|Select|](dt => (BaseType)dt);
                _ = derived.[|Select|](dt => (BaseType?)dt);

                _ = derived.[|Select|](i => (object)i);
                _ = derived.[|Select|](i => (object?)i);

                _ = derived.[|Select<DerivedType, object>|](i => i);
                _ = derived.[|Select<DerivedType, object?>|](i => i);

                _ = Enumerable.Range(0, 1).[|Select<int, object>|](i => i);
                _ = Enumerable.Range(0, 1).[|Select<int, object?>|](i => i);
            }
        }
        """, """
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public class BaseType { }
            public class DerivedType : BaseType { }
        
            public static void Run(IEnumerable<DerivedType> derived)
            {
                _ = derived.Cast<BaseType>();
                _ = derived.Cast<BaseType?>();
        
                _ = derived.Cast<object>();
                _ = derived.Cast<object?>();
        
                _ = derived.Cast<object>();
                _ = derived.Cast<object?>();
        
                _ = Enumerable.Range(0, 1).Cast<object>();
                _ = Enumerable.Range(0, 1).Cast<object?>();
            }
        }
        """);

    [TestMethod]
    public Task DontWarnOnMultipleCastAsync() => VerifyAsync("""
        using System.Linq;
        using System.Collections.Generic;
        
        public static class Test
        {
            public class BaseType { }
            public class DerivedType : BaseType { }
        
            public static void Run(IEnumerable<DerivedType> derived)
            {
                _ = derived.Cast<BaseType>();
                _ = derived.Cast<BaseType?>();
        
                _ = derived.Cast<object>();
                _ = derived.Cast<object?>();
        
                _ = derived.Cast<object>();
                _ = derived.Cast<object?>();
        
                _ = Enumerable.Range(0, 1).Cast<object>();
                _ = Enumerable.Range(0, 1).Cast<object?>();
            }
        }
        """);
}
