namespace Creedengo.Core.Analyzers;

/// <summary>GCIXXX: Unnecessary assignment.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnecessaryAssignment : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [SyntaxKind.SimpleAssignmentExpression, SyntaxKind.AddAssignmentExpression];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCIXX_UnecessaryAssignment,
        title: "Unnecessary assignment",
        message: "Assignment is not necessary.",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Info,
        description: "Assignments shoult not be made when not necessary.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.RegisterSyntaxNodeAction(static context => AnalyzeInvocationExpression(context), SyntaxKinds);
    }

    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        var assignmentExpr = (AssignmentExpressionSyntax)context.Node;

        if (assignmentExpr.Left is not IdentifierNameSyntax identifierName ||
            context.SemanticModel.GetSymbolInfo(assignmentExpr.Left).Symbol is not ILocalSymbol localSymbol ||
            localSymbol.DeclaringSyntaxReferences.Length != 1)
        {
            return;
        }

        var methodDeclaration = assignmentExpr.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration == null)
        {
            return;
        }

        var assignments = methodDeclaration.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>()
            .Where(a => context.SemanticModel.GetSymbolInfo(a.Left).Symbol?.Equals(localSymbol) == true)
            .ToList();

        if (assignments.Count > 1 && assignments.Where(a => context.SemanticModel.GetConstantValue(a.Right).HasValue).Count() > 1)
        {
            //for (int i = 0; i < assignments.Count; i++)
            //{
            //    var currentAssignment = assignments[i];
            //    //var previousAssignment = assignments[i - 1];

            //    if (context.SemanticModel.GetConstantValue(currentAssignment.Right).HasValue && )
            //    {
            //        context.ReportDiagnostic(Diagnostic.Create(Descriptor, currentAssignment.GetLocation()));
            //    }
            //}

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignmentExpr.GetLocation()));
        }
    }
}
