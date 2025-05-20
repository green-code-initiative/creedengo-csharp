
namespace Creedengo.Tests.Tests;

/// <summary>GCI92: Use Length to test empty strings.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseIsOperatorInsteadOfAsOperator : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [];

    public override void Initialize(AnalysisContext context){}
}
