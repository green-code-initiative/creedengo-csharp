namespace EcoCode.Analyzers;

/// <summary>GCI75: Don't concatenate strings in loops.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DontConcatenateStringsInLoops : DiagnosticAnalyzer
{
    private static readonly ImmutableArray<SyntaxKind> SyntaxKinds = [
        SyntaxKind.ForStatement,
        SyntaxKind.ForEachStatement,
        SyntaxKind.WhileStatement,
        SyntaxKind.DoStatement];
    private static readonly ImmutableArray<OperationKind> Invocations = [OperationKind.Invocation];

    /// <summary>The diagnostic descriptor.</summary>
    public static DiagnosticDescriptor Descriptor { get; } = Rule.CreateDescriptor(
        id: Rule.Ids.GCI75_DontConcatenateStringsInLoops,
        title: "Don't concatenate strings in loops",
        message: "A string is concatenated in a loop",
        category: Rule.Categories.Performance,
        severity: DiagnosticSeverity.Warning,
        description: "Strings should not be concatenated in loops, use a more optimized way, such as a StringBuilder.");

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => _supportedDiagnostics;
    private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics = [Descriptor];

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static context => AnalyzeLoopNode(context), SyntaxKinds);
        context.RegisterOperationAction(static context => AnalyzeForEach(context), Invocations);
    }

    private static void AnalyzeLoopNode(SyntaxNodeAnalysisContext context)
    {
        foreach (var loopStatement in context.Node.GetLoopStatements())
        {
            if (loopStatement is not ExpressionStatementSyntax { Expression: AssignmentExpressionSyntax assignment } ||
                assignment.Left is not IdentifierNameSyntax identifierName ||
                context.SemanticModel.GetSymbolInfo(identifierName).Symbol is not ISymbol symbol ||
                !symbol.IsVariableOfType(SpecialType.System_String)) // The assigned symbol must be a string
            {
                continue;
            }

            // AddAssignmentExpression corresponds to : a += b. We know we can warn at this point
            // SimpleAssignmentExpression corresponds to : a = b. In this case, check that
            // the right term is an addition and the assigned symbol is one of the operands
            bool report = assignment.IsKind(SyntaxKind.AddAssignmentExpression) ||
                assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                assignment.Right is BinaryExpressionSyntax binExpr &&
                binExpr.IsKind(SyntaxKind.AddExpression) &&
                (SymbolEqualityComparer.Default.Equals(symbol, context.SemanticModel.GetSymbolInfo(binExpr.Left).Symbol) ||
                 SymbolEqualityComparer.Default.Equals(symbol, context.SemanticModel.GetSymbolInfo(binExpr.Right).Symbol));

            if (report && symbol.IsDeclaredOutsideLoop(context.Node)) // Check IsDeclaredOutsideLoop last as it requires more work
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, assignment.GetLocation()));
        }
    }

    private static void AnalyzeForEach(OperationAnalysisContext context)
    {
        if (context.Operation is not IInvocationOperation { TargetMethod.Name: "ForEach" } operation ||
            GetBodyDelegateOperation(operation, context.Compilation)?.Value is not IDelegateCreationOperation { Target: { } body })
        {
            return;
        }

        foreach (var op in body.Descendants())
        {
            var target = default(IOperation);
            if (op is ICompoundAssignmentOperation compoundAssign) // a += b
            {
                if (compoundAssign.OperatorKind is BinaryOperatorKind.Add && compoundAssign.Target.Type?.SpecialType is SpecialType.System_String)
                    target = compoundAssign.Target;
            }
            else if (op is ISimpleAssignmentOperation simpleAssign && // a = b
                simpleAssign.Target.Type?.SpecialType is SpecialType.System_String &&
                simpleAssign.Value is IBinaryOperation { OperatorKind: BinaryOperatorKind.Add } binOp &&
                simpleAssign.MatchesAnyOperand(binOp.LeftOperand, binOp.RightOperand))
            {
                target = simpleAssign.Target;
            }
            if (target is null) continue;

            if (target is not ILocalReferenceOperation localRef ||
                localRef.Local.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is { } declaringSyntax &&
                !body.Syntax.Span.Contains(declaringSyntax.Span)) // Local variable is a capture, declared outside of the method body
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, op.Syntax.GetLocation()));
            }
        }

        static IArgumentOperation? GetBodyDelegateOperation(IInvocationOperation operation, Compilation compilation)
        {
            var symbol = operation.TargetMethod.ContainingType.OriginalDefinition;
            if (operation.TargetMethod.ContainingType.IsStatic) // Parallel.ForEach<T>, the delegate to analyze is always called 'body'
            {
                if (SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Threading.Tasks.Parallel")))
                    return operation.Arguments.FirstOrDefault(a => a.Parameter?.Name == "body");
            }
            else if (operation.TargetMethod.IsStatic) // Array : static void ForEach<T>(T[] array, Action<T> action)
            {
                if (SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Array")))
                    return operation.Arguments[1];
            }
            else if ( // List<T> and ImmutableList<T> : void ForEach(Action<T> action)
                SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Collections.Generic.List`1")) ||
                SymbolEqualityComparer.Default.Equals(symbol, compilation.GetTypeByMetadataName("System.Collections.Immutable.ImmutableList`1")))
            {
                return operation.Arguments[0];
            }
            return null;
        }
    }
}
