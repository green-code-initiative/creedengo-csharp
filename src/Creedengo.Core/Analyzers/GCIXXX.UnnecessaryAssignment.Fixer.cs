namespace Creedengo.Core.Analyzers;

/// <summary>GCI87 fixer: Use list indexer.</summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseListIndexerFixer)), Shared]
public sealed class UnnecessaryAssignmentFixer : CodeFixProvider
{
    /// <inheritdoc/>
    public override ImmutableArray<string> FixableDiagnosticIds => _fixableDiagnosticIds;
    private static readonly ImmutableArray<string> _fixableDiagnosticIds = [UnecessaryAssignment.Descriptor.Id];

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    /// <inheritdoc/>
    public override Task RegisterCodeFixesAsync(CodeFixContext context) => Task.CompletedTask;
}
