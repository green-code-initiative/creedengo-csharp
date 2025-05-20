namespace Creedengo.Core.Analyzers;

/// <summary>GCIXXX: Unnecessary assignment.</summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UnecessaryAssignment : DiagnosticAnalyzer
{
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
        context.RegisterSyntaxNodeAction(static context => AnalyzeIfStatement(context), SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(static context => AnalyzeSwitchStatement(context), SyntaxKind.SwitchStatement);
    }

    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        var ifStatement = (IfStatementSyntax)context.Node;

        if (ifStatement.IsSimpleIf())
            return;

        var parent = ifStatement.Parent;
        if (parent is null)
            return;

        SyntaxList<StatementSyntax>? statements = parent.Kind() switch
        {
            SyntaxKind.Block => ((BlockSyntax)parent).Statements,
            SyntaxKind.SwitchSection => ((SwitchSectionSyntax)parent).Statements,
            _ => null,
        };

        if (statements is null)
            return;

        var returnStatement = FindReturnStatementBelow(statements.Value, ifStatement);

        var expression = returnStatement?.Expression;

        if (returnStatement is null || expression is null)
            return;

        if (ifStatement.SpanOrTrailingTriviaContainsDirectives())
            return;

        if (returnStatement.SpanOrLeadingTriviaContainsDirectives())
            return;

        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        var symbol = context.SemanticModel.GetSymbolInfo(expression).Symbol;

        if (symbol is null)
            return;

        if (!IsLocalDeclaredInScopeOrNonRefOrOutParameterOfEnclosingSymbol(symbol, parent, semanticModel, cancellationToken))
            return;

        var returnTypeSymbol = semanticModel
            .GetTypeInfo(expression, cancellationToken)
            .Type;

        if (returnTypeSymbol is null)
            return;

        var current = ifStatement;
        while (current != null)
        {
            var statement = current.Statement;

            if (statement.IsKind(SyntaxKind.Block))
                statement = ((BlockSyntax)statement).Statements.LastOrDefault();

            if (statement is null)
                return;

            if (!statement.IsKind(SyntaxKind.ThrowStatement)
                && !IsSymbolAssignedInStatementWithCorrectType(symbol, statement, semanticModel, returnTypeSymbol, cancellationToken))
            {
                return;
            }

            current = current.Else?.Statement as IfStatementSyntax;
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, ifStatement.GetLocation()));
    }

    private static void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext context)
    {
        var switchStatement = (SwitchStatementSyntax)context.Node;

        var parent = switchStatement.Parent;
        if (parent is null)
            return;
        SyntaxList<StatementSyntax>? statements = parent.Kind() switch
        {
            SyntaxKind.Block => ((BlockSyntax)parent).Statements,
            SyntaxKind.SwitchSection => ((SwitchSectionSyntax)parent).Statements,
            _ => null,
        };

        if (statements is null)
            return;

        var returnStatement = FindReturnStatementBelow(statements.Value, switchStatement);

        if (returnStatement is null)
            return;

        var expression = returnStatement.Expression;

        if (expression is null)
            return;

        if (switchStatement.SpanOrTrailingTriviaContainsDirectives())
            return;

        if (returnStatement.SpanOrLeadingTriviaContainsDirectives())
            return;

        var semanticModel = context.SemanticModel;
        var cancellationToken = context.CancellationToken;

        var symbol = semanticModel
            .GetSymbolInfo(expression, cancellationToken)
            .Symbol;

        var returnTypeSymbol = semanticModel.GetTypeInfo(expression, cancellationToken).Type;

        if (symbol is null || returnTypeSymbol is null)
            return;

        if (!IsLocalDeclaredInScopeOrNonRefOrOutParameterOfEnclosingSymbol(symbol, parent, semanticModel, cancellationToken))
            return;

        foreach (var section in switchStatement.Sections)
        {
            var statements2 = section.Statements;

            if (statements2.SingleOrDefaultNoThrow() is BlockSyntax block)
            {
                statements2 = block.Statements;
            }

            if (!statements2.Any())
                return;

            switch (statements2.Last().Kind())
            {
                case SyntaxKind.ThrowStatement:
                    {
                        continue;
                    }
                case SyntaxKind.BreakStatement:
                    {
                        if (statements2.Count == 1
                            || !IsSymbolAssignedInStatementWithCorrectType(symbol, statements2[statements2.Count - 2], semanticModel, returnTypeSymbol, cancellationToken))
                        {
                            return;
                        }

                        break;
                    }
                default:
                    {
                        return;
                    }
            }
        }

        context.ReportDiagnostic(Diagnostic.Create(Descriptor, switchStatement.GetLocation()));
    }

    internal static ReturnStatementSyntax? FindReturnStatementBelow(SyntaxList<StatementSyntax> statements, StatementSyntax statement)
    {
        int index = statements.IndexOf(statement);

        if (index < statements.Count - 1)
        {
            var nextStatement = statements[index + 1];

            if (nextStatement.IsKind(SyntaxKind.ReturnStatement))
                return (ReturnStatementSyntax)nextStatement;
        }

        return null;
    }

    private static bool IsLocalDeclaredInScopeOrNonRefOrOutParameterOfEnclosingSymbol(ISymbol symbol, SyntaxNode containingNode, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        switch (symbol.Kind)
        {
            case SymbolKind.Local:
                {
                    var localSymbol = (ILocalSymbol)symbol;

                    var localDeclarationStatement = localSymbol.GetSyntax(cancellationToken).Parent?.Parent as LocalDeclarationStatementSyntax;

                    return localDeclarationStatement?.Parent == containingNode;
                }
            case SymbolKind.Parameter:
                {
                    var parameterSymbol = (IParameterSymbol)symbol;

                    if (parameterSymbol.RefKind == RefKind.None)
                    {
                        var enclosingSymbol = semanticModel.GetEnclosingSymbol(containingNode.SpanStart, cancellationToken);

                        if (enclosingSymbol is not null)
                        {
                            var parameters = enclosingSymbol.Kind switch
                            {
                                SymbolKind.Method => ((IMethodSymbol)symbol).Parameters,
                                SymbolKind.Property => ((IPropertySymbol)symbol).Parameters,
                                _ => default
                            };

                            return !parameters.IsDefault
                                && parameters.Contains(parameterSymbol);
                        }
                    }

                    break;
                }
        }

        return false;
    }

    private static bool IsSymbolAssignedInStatementWithCorrectType(ISymbol symbol, StatementSyntax statement, SemanticModel semanticModel, ITypeSymbol typeSymbol, CancellationToken cancellationToken)
    {
        var expression = statement is ExpressionStatementSyntax syntax ? syntax.Expression : null;
        if (expression is not AssignmentExpressionSyntax assignmentExpression)
            return false;

        var leftSymbol = semanticModel
        .GetSymbolInfo(assignmentExpression.Left, cancellationToken)
        .Symbol;

        var rightTypeSymbol = semanticModel
         .GetTypeInfo(assignmentExpression.Right, cancellationToken)
         .Type;

        return assignmentExpression is not null
            && SymbolEqualityComparer.Default.Equals(leftSymbol, symbol)
            && SymbolEqualityComparer.Default.Equals(typeSymbol, rightTypeSymbol);
    }
}
