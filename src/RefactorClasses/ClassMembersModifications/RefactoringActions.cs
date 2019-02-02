using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using RefactorClasses.RoslynUtils.SemanticAnalysis.Class;
using RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.ClassMembersModifications
{
    public static class RefactoringActions
    {
        public static async Task<Document> AddParameterToConstructor(
            Document document,
            ClassDeclarationSyntax classDeclaration,
            AnalysedDeclaration analysedDeclaration, // must be either field variable or property
            IMethodSymbol constructorSymbol,
            ConstructorDeclarationSyntax constructorDeclaration,
            CancellationToken cancellationToken)
        {
            const int AppendPosition = int.MaxValue;

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var classMemberAnalysis = new ClassMembersAnalysis(classDeclaration, semanticModel);
            var analyser = new ConstructorPropertyRelationshipAnalyser(
                classMemberAnalysis.Fields.Select(f => f.Symbol).ToArray(),
                classMemberAnalysis.Properties.Select(f => f.Symbol).ToArray());
            var result = analyser.Analyze(semanticModel, constructorDeclaration);

            // Filter to consider only fields and properties that are assigned in constructor.
            var filteredClassMembers = classMemberAnalysis.WithFilteredProperties(
                (in FieldInfo fi) => result.GetResult(fi.Symbol) is AssignmentExpressionAnalyserResult,
                (in PropertyInfo pi) => result.GetResult(pi.Symbol) is AssignmentExpressionAnalyserResult);

            // Find closest declaration among the ones that are assigned in the constructor
            var (closestSymbol, isBeforeFoundSymbol) = filteredClassMembers.GetClosestFieldOrProperty(analysedDeclaration.Symbol);
            int constructorInsertPosition = AppendPosition;

            if (closestSymbol != null)
            {
                // There is another member that is assigned in constructor
                constructorInsertPosition = result.GetMatchingParameterIdx(constructorSymbol, closestSymbol);
                if (!isBeforeFoundSymbol) ++constructorInsertPosition;
            }

            // TODO: resolve name clashes if parameter with a given name already exists?
            var addedParameter = SyntaxHelpers.LowercaseIdentifierFirstLetter(analysedDeclaration.Identifier);
            var newConstructorDeclaration = constructorDeclaration.InsertParameter(
                SyntaxHelpers.Parameter(
                    analysedDeclaration.Type,
                    addedParameter),
                constructorInsertPosition);

            AssignmentExpressionSyntax closestSymbolAssignment = null;
            if (closestSymbol != null && result.GetResult(closestSymbol) is AssignmentExpressionAnalyserResult res)
            {
                closestSymbolAssignment = res.Assignment;
            }

            var fieldIdentifier = SyntaxFactory.IdentifierName(analysedDeclaration.Identifier);
            var parameterIdentifier = SyntaxFactory.IdentifierName(addedParameter);
            var leftSide = addedParameter.ValueText.Equals(analysedDeclaration.Identifier.ValueText, StringComparison.Ordinal) ?
                (ExpressionSyntax)ExpressionGenerationHelper.ThisMemberAccess(fieldIdentifier)
                : fieldIdentifier;
            var assignment = ExpressionGenerationHelper.SimpleAssignment(leftSide, parameterIdentifier);

            var statementToAdd = SyntaxFactory.ExpressionStatement(assignment);
            var body = constructorDeclaration.Body;

            var closestStatement = closestSymbolAssignment?.Parent as ExpressionStatementSyntax;
            if (closestStatement == null)
            {
                newConstructorDeclaration = newConstructorDeclaration
                    .AddBodyStatements(statementToAdd)
                    .WithAdditionalAnnotations(Formatter.Annotation);
            }
            else if (isBeforeFoundSymbol)
            {
                var newBody = body.InsertBefore(closestStatement, statementToAdd);
                newConstructorDeclaration = newConstructorDeclaration.WithBody(
                    newBody).WithAdditionalAnnotations(Formatter.Annotation);
            }
            else
            {
                var newBody = body.InsertAfter(closestStatement, statementToAdd);
                newConstructorDeclaration = newConstructorDeclaration.WithBody(
                    newBody).WithAdditionalAnnotations(Formatter.Annotation);
            }

            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(constructorDeclaration, newConstructorDeclaration);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static List<(T syntax, U symbol)> FilterAssignedSymbols<T, U>(
            T[] syntaxDeclarations,
            U[] symbols,
            ConstructorPropertyRelationshipAnalyserResult result)
            where U : ISymbol
        {
            if (symbols.Length != syntaxDeclarations.Length) throw new ArgumentException("symbols.Length != syntaxDeclarations.Length");

            var r = new List<(T, U)>();
            for (int i = 0; i < syntaxDeclarations.Length; ++i)
            {
                var s = symbols[i];
                if (result.GetResult(s) is AssignmentExpressionAnalyserResult)
                {
                    r.Add((syntaxDeclarations[i], s));
                }
            }

            return r;
        }

        private static (T element, int distance) FindClosestElement<T>(
            List<T> declarations,
            Func<T, bool> shouldSkip,
            Func<T, int> getDistance)
        {
            T selected = default(T);
            int minDistance = int.MaxValue;

            foreach (var d in declarations)
            {
                if (shouldSkip(d)) continue;

                var dist = getDistance(d);
                if (dist > 0 && dist < minDistance)
                {
                    minDistance = dist;
                    selected = d;
                }
            }

            return (selected, minDistance);
        }
    }
}
