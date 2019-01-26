using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors
{

    /// <summary>
    /// <see cref="ConstructorPropertyRelationshipAnalyser"/> helps in answering questions
    /// which property or field is set in constructor and how.
    /// </summary>
    public sealed class ConstructorPropertyRelationshipAnalyser
    {
        private readonly IFieldSymbol[] fields;
        private readonly IPropertySymbol[] properties;

        public ConstructorPropertyRelationshipAnalyser(
            IFieldSymbol[] fields,
            IPropertySymbol[] properties)
        {
            this.fields = fields ?? throw new ArgumentNullException(nameof(fields));
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));

            // Try to handle both 'this.prop = prop1;' and 'prop = prop1;'
            // TODO: exclude constant fields ?
            // TODO: test with arrow expression constructors ?
            // TODO: test with assignment inside if(....) {  } else { } / switch() ... etc.
            // TODO: Ensure it works assignments like A = a ?? throw or A = a == null ? xxx : yyy or ???
            // TODO: What to do in case of 
            // TODO: handle things like (a, b, c) = (10, 20, 30);
            // Exclude constructors with : this() ???
            // Constructors that call base ?
        }

        public ConstructorPropertyRelationshipAnalyserResult Analyze(
            SemanticModel documentModel,
            ConstructorDeclarationSyntax constructorDeclaration)
        {
            var searcher = new AssignmentSearcher(documentModel, fields, properties);
            searcher.Visit(constructorDeclaration);
            return searcher.GetResult();
        }

        public class ConstructorPropertyRelationshipAnalyserResult
        {
            private static readonly EmptyAssignmentAnalyserResult EmptyResult = new EmptyAssignmentAnalyserResult();

            private Dictionary<IFieldSymbol, AssignmentAnalyserResult> fieldResults;
            private Dictionary<IPropertySymbol, AssignmentAnalyserResult> propertyResults;

            public ConstructorPropertyRelationshipAnalyserResult(
                Dictionary<IFieldSymbol, AssignmentAnalyserResult> fieldResults,
                Dictionary<IPropertySymbol, AssignmentAnalyserResult> propertyResults)
            {
                this.fieldResults = fieldResults;
                this.propertyResults = propertyResults;
            }

            public AssignmentAnalyserResult GetResult(IFieldSymbol fieldSymbol)
            {
                //if (fieldSymbol == null || !fields.Contains(fieldSymbol))
                //    return EmptyResult;

                return EmptyResult;
            }

            public AssignmentAnalyserResult GetResult(IPropertySymbol propertySymbol)
            {
                //if (propertySymbol == null || !properties.Contains(propertySymbol))
                //    return EmptyResult;

                return EmptyResult;
            }
        }

        public abstract class AssignmentAnalyserResult { }

        public sealed class EmptyAssignmentAnalyserResult : AssignmentAnalyserResult { }

        /// <summary>
        /// <see cref="MultipleAssignments"/> represents a case in which property is assigned multiple times
        /// or in an if expression.
        /// </summary>
        public sealed class MultipleAssignments : AssignmentAnalyserResult { }

        /// <summary>
        /// <see cref="ParsingError"/> means that the assignment expression was to complicated for current parser.
        /// </summary>
        public sealed class ParsingError : AssignmentAnalyserResult { }

        public sealed class AssignmentExpressionAnalyserResult : AssignmentAnalyserResult
        {
            public AssignmentExpressionAnalyserResult(
                AssignmentExpressionSyntax assignment,
                IParameterSymbol assignedParameter)
            {
                Assignment = assignment;
                AssignedParameter = assignedParameter;
            }

            /// <summary>
            /// Expression which assigns to a result.
            /// </summary>
            public AssignmentExpressionSyntax Assignment { get; }

            /// <summary>
            /// Constructor parameter that is assigned to field.
            /// </summary>
            public IParameterSymbol AssignedParameter { get; }
        }

        private class AssignmentSearcher : CSharpSyntaxWalker
        {
            private static readonly AssignmentAnalyserResult MultipleAssignments = new MultipleAssignments();
            private static readonly AssignmentAnalyserResult ParsingError = new ParsingError();

            private readonly SemanticModel semanticModel;
            private readonly IFieldSymbol[] fields;
            private readonly IPropertySymbol[] properties;

            private readonly Dictionary<ISymbol, AssignmentAnalyserResult> foundAssignments = new Dictionary<ISymbol, AssignmentAnalyserResult>();

            public AssignmentSearcher(
                SemanticModel semanticModel,
                IFieldSymbol[] fields,
                IPropertySymbol[] properties)
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
                if ((fields.Length == 0 && properties.Length == 0)
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

                            return Analyse(exp.WhenTrue);

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
}
