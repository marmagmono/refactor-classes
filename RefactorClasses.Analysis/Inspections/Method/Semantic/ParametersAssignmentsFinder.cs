using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Method.Semantic
{
    internal class ParametersAssignmentsFinder : CSharpSyntaxWalker
    {
        private readonly IReadOnlyList<IParameterSymbol> parameters;
        private readonly SemanticModel semanticModel;
        private readonly List<Assignment> assignments;

        public ParametersAssignmentsFinder(
            IReadOnlyList<IParameterSymbol> parameters,
            SemanticModel semanticModel)
        {
            this.parameters = parameters;
            this.semanticModel = semanticModel;
            this.assignments = new List<Assignment>();
        }

        public AssignmentsResult GetAssignedParameters()
        {
            var notAssignedParameters = new List<NotAssignedParameter>();
            foreach ((IParameterSymbol p, int i) in this.parameters.Select((p, i) => (p, i)))
            {
                if (!assignments.Any(a => a.Parameter.Equals(p)))
                {
                    notAssignedParameters.Add(new NotAssignedParameter(i, p));
                }
            }

            return new AssignmentsResult(this.assignments, notAssignedParameters);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            // TODO: local declarations

            if ((this.parameters.Count == 0)
                || !node.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            var identifierName = TryGetIdentifier(node.Left);
            if (identifierName == null) return;

            var assignedSymbolInfo = semanticModel.GetSymbolInfo(identifierName);
            if (assignedSymbolInfo.Symbol == null) return;

            var property = AnalyzeAssignmentRight(this.semanticModel, node);
            if (property.Result == Result.Error) return;

            // try to match found property with given method properties.
            foreach ((IParameterSymbol p, int i) in this.parameters.Select((p, i) => (p, i)))
            {
                if (Equals(property.FoundSymbol.OriginalDefinition, p))
                {
                    this.assignments.Add(new Assignment(i, p, assignedSymbolInfo.Symbol, node));
                    break;
                }
            }

            base.VisitAssignmentExpression(node);
        }

        private AnalyzeAssignmentRightResult AnalyzeAssignmentRight(
            SemanticModel semanticModel,
            AssignmentExpressionSyntax assignmentSyntax)
        {
            // TODO: tuples
            // TODO: not only throws ?
            // = parameter;

            // = parameter ?? throw ();
            // = parameter1 ?? parameter2;
            // = not parameter;

            AnalyzeAssignmentRightResult Analyse(ExpressionSyntax expression)
            {
                switch (expression)
                {
                    case IdentifierNameSyntax identifer: // = parameter;
                        return FromIdentifierName(identifer);

                    case BinaryExpressionSyntax exp
                        when exp.IsKind(SyntaxKind.CoalesceExpression)
                        && exp.Right is ThrowExpressionSyntax: // ?? throw ...

                        return Analyse(exp.Left);

                    case ConditionalExpressionSyntax exp
                        when exp.WhenFalse is ThrowExpressionSyntax: // aaa = fdfs ? o1 : throw;

                        return Analyse(exp.WhenTrue);

                    case ConditionalExpressionSyntax exp
                        when exp.WhenTrue is ThrowExpressionSyntax: // aaa = fdfs ? throw : o2;

                        return Analyse(exp.WhenFalse);

                    default:
                        return new AnalyzeAssignmentRightResult(Result.Error, null);
                }
            }

            AnalyzeAssignmentRightResult FromIdentifierName(IdentifierNameSyntax idName)
            {
                var parameter = semanticModel.GetSymbolInfo(idName).Symbol as IParameterSymbol;
                return parameter != null ?
                    new AnalyzeAssignmentRightResult(Result.Ok, parameter)
                    : new AnalyzeAssignmentRightResult(Result.Error, null);
            }

            return Analyse(assignmentSyntax.Right);
        }

        private IdentifierNameSyntax TryGetIdentifier(ExpressionSyntax expression)
        {
            // TODO: tuples
            switch (expression)
            {
                case IdentifierNameSyntax identifier:
                    return identifier;
                case MemberAccessExpressionSyntax memberAccess:
                    return memberAccess.Name as IdentifierNameSyntax;
                default:
                    return null;
            }
        }

        private enum Result { Error, Ok }

        private readonly struct AnalyzeAssignmentRightResult
        {
            public readonly Result Result;

            public readonly IParameterSymbol FoundSymbol;

            public AnalyzeAssignmentRightResult(Result result, IParameterSymbol foundSymbol)
            {
                Result = result;
                FoundSymbol = foundSymbol;
            }
        }
    }
}
