using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors
{
    internal class AssignmentSearcher : CSharpSyntaxWalker
    {
        private static readonly AssignmentAnalyserResult MultipleAssignments = new MultipleAssignments();
        private static readonly AssignmentAnalyserResult ParsingError = new ParsingError();

        private readonly SemanticModel semanticModel;
        private readonly IReadOnlyCollection<IFieldSymbol> fields;
        private readonly IReadOnlyCollection<IPropertySymbol> properties;

        private readonly Dictionary<ISymbol, AssignmentAnalyserResult> foundAssignments = new Dictionary<ISymbol, AssignmentAnalyserResult>();

        public AssignmentSearcher(
            SemanticModel semanticModel,
            IReadOnlyCollection<IFieldSymbol> fields,
            IReadOnlyCollection<IPropertySymbol> properties)
        {
            this.semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            this.fields = fields ?? throw new ArgumentNullException(nameof(fields));
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            // TODO: handle things like (a, b, c) = (10, 20, 30);
        }

        public ConstructorPropertyRelationshipAnalyserResult GetResult()
        {
            Dictionary<IFieldSymbol, AssignmentAnalyserResult> fieldResults =
                new Dictionary<IFieldSymbol, AssignmentAnalyserResult>();
            Dictionary<IPropertySymbol, AssignmentAnalyserResult> propertyResults =
                new Dictionary<IPropertySymbol, AssignmentAnalyserResult>();

            foreach (var kv in foundAssignments)
            {
                if (kv.Key is IFieldSymbol fieldSymbol)
                {
                    fieldResults.Add(fieldSymbol, kv.Value);
                }
                else if (kv.Key is IPropertySymbol propertySymbol)
                {
                    propertyResults.Add(propertySymbol, kv.Value);
                }
            }

            return new ConstructorPropertyRelationshipAnalyserResult(fieldResults, propertyResults);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if ((fields.Count == 0 && properties.Count == 0)
                || !node.IsKind(SyntaxKind.SimpleAssignmentExpression))
                return;

            // Try to handle both 'this.prop = prop1;' and 'prop = prop1;'
            var identifierName = TryGetIdentifier(node.Left);
            if (identifierName == null) return;

            // In case of member access expression it is not yet
            // validated that it is a this member

            var idString = identifierName.Identifier.ValueText;
            var matchingField = fields.FirstOrDefault(f => f.Name.Equals(idString, StringComparison.Ordinal));
            var matchingProperty = properties.FirstOrDefault(p => p.Name.Equals(idString, StringComparison.Ordinal));

            if ((matchingField == null && matchingProperty == null)
                || (matchingField != null && matchingProperty != null))
                return;

            var symbolInfo = semanticModel.GetSymbolInfo(identifierName);
            if (symbolInfo.Symbol == null) return;

            ISymbol matchedSymbol = (ISymbol)matchingField ?? matchingProperty;
            if (Equals(symbolInfo.Symbol.OriginalDefinition, matchedSymbol))
            {
                if (foundAssignments.ContainsKey(matchedSymbol))
                {
                    foundAssignments[matchedSymbol] = MultipleAssignments;
                }
                else
                {
                    foundAssignments[matchedSymbol] = AnalyzeAssignmentRight(semanticModel, node);
                }
            }

            base.VisitAssignmentExpression(node);
        }

        private AssignmentAnalyserResult AnalyzeAssignmentRight(
            SemanticModel semanticModel,
            AssignmentExpressionSyntax assignmentSyntax)
        {
            // = parameter;

            // = parameter ?? throw ();
            // = parameter1 ?? parameter2;
            // = not parameter;

            AssignmentAnalyserResult Analyse(ExpressionSyntax expression)
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
                        return ParsingError;
                }
            }

            AssignmentAnalyserResult FromIdentifierName(IdentifierNameSyntax idName)
            {
                var parameter = semanticModel.GetSymbolInfo(idName).Symbol as IParameterSymbol;
                return parameter != null ?
                    new AssignmentExpressionAnalyserResult(assignmentSyntax, parameter)
                    : ParsingError;
            }

            return Analyse(assignmentSyntax.Right);
        }

        private IdentifierNameSyntax TryGetIdentifier(ExpressionSyntax expression)
        {
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
    }
}
