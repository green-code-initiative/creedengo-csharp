using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace Creedengo.Tests;

public delegate Task AnalyzerDlg(string source, LanguageVersion languageVersion = default);
public delegate Task CodeFixerDlg(string source, string? fixedSource = null, LanguageVersion languageVersion = default);

internal static class TestRunner
{
    // Analyzer

    public static Task VerifyAsync<TAnalyzer>(string source, LanguageVersion languageVersion = default)
        where TAnalyzer : DiagnosticAnalyzer, new() =>
        new CustomAnalyzerVerifier<TAnalyzer>(source, languageVersion).RunAsync();

    private sealed class CustomAnalyzerVerifier<TAnalyzer> : CSharpAnalyzerTest<TAnalyzer, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        private readonly LanguageVersion _languageVersion;

        public CustomAnalyzerVerifier(string source, LanguageVersion languageVersion)
        {
            TestCode = source;
            _languageVersion = languageVersion;
        }

        protected override ParseOptions CreateParseOptions() => _languageVersion == default
            ? base.CreateParseOptions()
            : ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(_languageVersion);
    }

    // CodeFix

    public static Task VerifyAsync<TAnalyzer, TCodeFix>(string source, string? fixedSource = null, LanguageVersion languageVersion = default)
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new() =>
        new CustomCodeFixVerifier<TAnalyzer, TCodeFix>(source, fixedSource, languageVersion).RunAsync();

    private sealed class CustomCodeFixVerifier<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
        where TAnalyzer : DiagnosticAnalyzer, new()
        where TCodeFix : CodeFixProvider, new()
    {
        private readonly LanguageVersion _languageVersion;

        public CustomCodeFixVerifier(string source, string? fixedSource, LanguageVersion languageVersion)
        {
            TestCode = source;
            if (fixedSource is not null) FixedCode = fixedSource;
            _languageVersion = languageVersion;
        }

        protected override ParseOptions CreateParseOptions() => _languageVersion == default
            ? base.CreateParseOptions()
            : ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(_languageVersion);
    }
}
