using Creedengo.Core.Analyzers;
using System.Xml.Linq;
namespace Creedengo.Tests.Tests;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReturnTaskDirectly)), Shared]
public sealed class UseIsOperatorInsteadOfAsOperatorFixer : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [];

    public override async Task RegisterCodeFixesAsync(CodeFixContext context) { }
}
