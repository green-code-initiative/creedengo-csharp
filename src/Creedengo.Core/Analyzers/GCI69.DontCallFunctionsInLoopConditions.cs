namespace Creedengo.Analyzers;

/// <summary>GCI69: Don't call loop invariant functions in loop conditions.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontCallFunctionsInLoopConditions : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [
        SyntaxKind.ForStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI69_DontCallFunctionsInLoopConditions,
        title: "Don't call loop invariant functions in loop conditions",
        message: "A loop invariant function is called in a loop condition",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Loop invariant functions should be resolved before loops to avoid rerunning them for every iteration.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeLoopNode(context), SyntaxKinds);
    }

    private static void AnalyzeLoopNode(SyntaxNodeAnalysisContext context)
    {
        var (condition, expression, incrementors) = context.Node switch
        {
            ForStatementSyntax forStatement => (forStatement.Condition, forStatement.Statement, forStatement.Incrementors),
            WhileStatementSyntax whileStatement => (whileStatement.Condition, whileStatement.Statement, default),
            DoStatementSyntax doStatement => (doStatement.Condition, doStatement.Statement, default),
            _ => default
        };
        if (condition is null) return;

        // Step 1: Identify the variables used in the conditions
        var loopInvariantSymbols = new HashSet<ISymbol>(SymbolEqualityComparer.Default);
        foreach (var node in condition.DescendantNodes())
        {
            if (node is not InvocationExpressionSyntax invocation) continue;

            // Analyze the object on which the method is called, if any
            var callee = invocation.Expression switch
            {
                MemberAccessExpressionSyntax m => m.Expression,
                MemberBindingExpressionSyntax => (node.Parent as ConditionalAccessExpressionSyntax)?.Expression, // ?. operator
                _ => null,
            };
            if (callee is not null && context.SemanticModel.GetSymbolInfo(callee).Symbol is ISymbol c && c.IsVariable())
                _ = loopInvariantSymbols.Add(c);

            // Analyze the arguments of the method call
            foreach (var arg in invocation.ArgumentList.Arguments)
            {
                if (context.SemanticModel.GetSymbolInfo(arg.Expression).Symbol is ISymbol s && s.IsVariable())
                    _ = loopInvariantSymbols.Add(s);
            }
        }

        // Step 2: Remove the variables that are mutated in the loop body and/or the for loop incrementors
        if (loopInvariantSymbols.Count != 0)
        {
            RemoveMutatedSymbols(expression.DescendantNodes(), loopInvariantSymbols, context.SemanticModel);
            foreach (var inc in incrementors)
                RemoveMutatedSymbols(inc.DescendantNodesAndSelf(), loopInvariantSymbols, context.SemanticModel);
        }

        // Step 3: Identify conditions that are loop invariant
        foreach (var node in condition.DescendantNodes())
        {
            if (node is not InvocationExpressionSyntax invocation) continue;

            if (invocation.Expression is MemberBindingExpressionSyntax memberBinding)
            {
                if (node.Parent is ConditionalAccessExpressionSyntax conditionalAccess &&
                    IsLoopInvariant(conditionalAccess.Expression) &&
                    invocation.ArgumentList.Arguments.All(arg => IsLoopInvariant(arg.Expression)))
                {
                    context.ReportDiagnostic(Diagnostic.Create(Descriptor, conditionalAccess.GetLocation()));
                }
                continue;
            }

            if ((invocation.Expression is not MemberAccessExpressionSyntax memberAccess || IsLoopInvariant(memberAccess.Expression)) &&
                invocation.ArgumentList.Arguments.All(arg => IsLoopInvariant(arg.Expression)))
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocation.GetLocation()));
            }
        }

        static void RemoveMutatedSymbols(IEnumerable<SyntaxNode> nodes, HashSet<ISymbol> loopInvariantSymbols, SemanticModel semanticModel)
        {
            if (loopInvariantSymbols.Count == 0) return;
            foreach (var node in nodes)
            {
                var symbol = node switch
                {
                    AssignmentExpressionSyntax assignment => semanticModel.GetSymbolInfo(assignment.Left).Symbol,
                    PrefixUnaryExpressionSyntax prefix => semanticModel.GetSymbolInfo(prefix.Operand).Symbol,
                    PostfixUnaryExpressionSyntax postfix => semanticModel.GetSymbolInfo(postfix.Operand).Symbol,
                    _ => null
                };

                if (symbol is not null && loopInvariantSymbols.Remove(symbol) && loopInvariantSymbols.Count == 0)
                    return;
            }
        }

        bool IsLoopInvariant(ExpressionSyntax expr) =>
            context.SemanticModel.GetSymbolInfo(expr).Symbol is not ISymbol s ||
            !s.IsVariable() ||
            loopInvariantSymbols.Contains(s);
    }
}
