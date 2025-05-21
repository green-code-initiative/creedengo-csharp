namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseEventArgsDotEmptyTests
{
    private static readonly CodeFixerDlg VerifyAndFixAsync = TestRunner.VerifyAsync<UseEventArgsDotEmpty, UseEventArgsDotEmptyFixer>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAndFixAsync(string.Empty);

    [TestMethod]
    public Task MinimalValidExampleCodeAsync() => VerifyAndFixAsync("""
        using System;
        class C
        {
            event EventHandler E;
            void M() => E?.Invoke(this, EventArgs.Empty);
        }
        """, """
        using System;
        class C
        {
            event EventHandler E;
            void M() => E?.Invoke(this, EventArgs.Empty);
        }
        """);

    [TestMethod]
    public Task MinimalInvalidExampleCodeAsync() => VerifyAndFixAsync("""
        using System;
        class C
        {
            event EventHandler E;
            void M() => E?.Invoke(this, [|new EventArgs()|]);
        }
        """, """
        using System;
        class C
        {
            event EventHandler E;
            void M() => E?.Invoke(this, EventArgs.Empty);
        }
        """);

    [TestMethod]
    public Task ValidExplicitInvocationUsingEmptyAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise() => E.Invoke(this, EventArgs.Empty);
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void Raise() => E.Invoke(this, EventArgs.Empty);
    }
    """);

    [TestMethod]
    public Task ValidEventArgsEmptyInVariableAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void M()
        {
            var args = EventArgs.Empty;
            E?.Invoke(this, args);
        }
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void M()
        {
            var args = EventArgs.Empty;
            E?.Invoke(this, args);
        }
    }
    """);

    [TestMethod]
    public Task ValidCustomEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    class MyArgs : EventArgs {}
    class C
    {
        event EventHandler<MyArgs> E;
        void M() => E?.Invoke(this, new MyArgs());
    }
    """, """
    using System;
    class MyArgs : EventArgs {}
    class C
    {
        event EventHandler<MyArgs> E;
        void M() => E?.Invoke(this, new MyArgs());
    }
    """);

    [TestMethod]
    public Task InvalidExplicitInvokeWithNewEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void M() => E.Invoke(this, [|new EventArgs()|]);
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void M() => E.Invoke(this, EventArgs.Empty);
    }
    """);

    [TestMethod]
    public Task InvalidEventArgsViaVariableAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void M()
        {
            var args = [|new EventArgs()|];
            E?.Invoke(this, args);
        }
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void M()
        {
            var args = EventArgs.Empty;
            E?.Invoke(this, args);
        }
    }
    """);

    [TestMethod]
    public Task InvalidHelperMethodPassingNewEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise(EventArgs e) => E?.Invoke(this, e);
        void M() => Raise([|new EventArgs()|]);
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void Raise(EventArgs e) => E?.Invoke(this, e);
        void M() => Raise(EventArgs.Empty);
    }
    """);


    [TestMethod]
    public Task InvalidLambdaExpressionWithNewEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    using System.Linq;
    class C
    {
        event EventHandler E;
        void M()
        {
            Action a = () => E?.Invoke(this, [|new EventArgs()|]);
        }
    }
    """, """
    using System;
    using System.Linq;
    class C
    {
        event EventHandler E;
        void M()
        {
            Action a = () => E?.Invoke(this, EventArgs.Empty);
        }
    }
    """);

    [TestMethod]
    public Task InvalidGenericMethodWithNewEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise<T>(T e) where T : EventArgs => E?.Invoke(this, e);
        void M() => Raise([|new EventArgs()|]);
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void Raise<T>(T e) where T : EventArgs => E?.Invoke(this, e);
        void M() => Raise(EventArgs.Empty);
    }
    """);

    [TestMethod]
    public Task InvalidNestedGenericMethodWithNewEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise<T>(T e) where T : EventArgs => E?.Invoke(this, e);
        void M() => Raise(L([|new EventArgs()|]));
        EventArgs L(EventArgs args) => args;
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void Raise<T>(T e) where T : EventArgs => E?.Invoke(this, e);
        void M() => Raise(L(EventArgs.Empty));
        EventArgs L(EventArgs args) => args;
    }
    """);

    [TestMethod]
    public Task MixedValidAndInvalidEventArgsAsync() => VerifyAndFixAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void M()
        {
            E?.Invoke(this, EventArgs.Empty);
            E?.Invoke(this, [|new EventArgs()|]);
        }
    }
    """, """
    using System;
    class C
    {
        event EventHandler E;
        void M()
        {
            E?.Invoke(this, EventArgs.Empty);
            E?.Invoke(this, EventArgs.Empty);
        }
    }
    """);
}
