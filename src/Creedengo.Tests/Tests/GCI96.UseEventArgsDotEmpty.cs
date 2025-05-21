namespace Creedengo.Tests.Tests;

[TestClass]
public sealed class UseEventArgsDotEmptyTests
{
    private static readonly AnalyzerDlg VerifyAsync = TestRunner.VerifyAsync<UseEventArgsDotEmpty>;

    [TestMethod]
    public Task EmptyCodeAsync() => VerifyAsync(string.Empty);

    [TestMethod]
    public Task MinimalValidExampleCodeAsync() => VerifyAsync("""
        using System;
        class C
        {
            event EventHandler E;
            void M() => E?.Invoke(this, EventArgs.Empty);
        }
        """);

    [TestMethod]
    public Task MinimalInvalidExampleCodeAsync() => VerifyAsync("""
        using System;
        class C
        {
            event EventHandler E;
            void M() => E?.Invoke(this, [|new EventArgs()|]);
        }
        """);

    [TestMethod]
    public Task ValidExplicitInvocationUsingEmptyAsync() => VerifyAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise() => E.Invoke(this, EventArgs.Empty);
    }
    """);

    [TestMethod]
    public Task ValidEventArgsEmptyInVariableAsync() => VerifyAsync("""
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
    public Task ValidCustomEventArgsAsync() => VerifyAsync("""
    using System;
    class MyArgs : EventArgs {}
    class C
    {
        event EventHandler<MyArgs> E;
        void M() => E?.Invoke(this, new MyArgs());
    }
    """);

    [TestMethod]
    public Task InvalidExplicitInvokeWithNewEventArgsAsync() => VerifyAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void M() => E.Invoke(this, [|new EventArgs()|]);
    }
    """);

    [TestMethod]
    public Task InvalidEventArgsViaVariableAsync() => VerifyAsync("""
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
    """);

    [TestMethod]
    public Task InvalidHelperMethodPassingNewEventArgsAsync() => VerifyAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise(EventArgs e) => E?.Invoke(this, e);
        void M() => Raise([|new EventArgs()|]);
    }
    """);


    [TestMethod]
    public Task InvalidLambdaExpressionWithNewEventArgsAsync() => VerifyAsync("""
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
    """);

    [TestMethod]
    public Task InvalidGenericMethodWithNewEventArgsAsync() => VerifyAsync("""
    using System;
    class C
    {
        event EventHandler E;
        void Raise<T>(T e) where T : EventArgs => E?.Invoke(this, e);
        void M() => Raise([|new EventArgs()|]);
    }
    """);

    [TestMethod]
    public Task MixedValidAndInvalidEventArgsAsync() => VerifyAsync("""
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
    """);
}
