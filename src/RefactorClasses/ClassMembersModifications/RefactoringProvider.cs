using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using RefactorClasses.RoslynUtils.SemanticAnalysis.Class;
using RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.ClassMembersModifications
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "ClassMembersModifications"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, classDeclarationSyntax) = await context.FindSyntaxForCurrentSpan<ClassDeclarationSyntax>();
            if (document == null || classDeclarationSyntax == null) return;

            if (ClassDeclarationSyntaxAnalysis.IsPartial(classDeclarationSyntax)
                || ClassDeclarationSyntaxAnalysis.IsStatic(classDeclarationSyntax))
            {
                return;
            }

            var (atMostOneNonTrivialConstructor, nonTrivialConstructorCandidate) =
                ClassDeclarationSyntaxAnalysis.HasAtMostOneNoneTrivialConstructor(classDeclarationSyntax);

            if (!atMostOneNonTrivialConstructor) return;

            var (_, propertyDeclaration) = await context.FindSyntaxForCurrentSpan<PropertyDeclarationSyntax>();
            var (_, fieldDeclaration) = await context.FindSyntaxForCurrentSpan<FieldDeclarationSyntax>();

            // TODO: Skip properties like string Prop => "something";
            if ((fieldDeclaration == null && propertyDeclaration == null)
                || (fieldDeclaration != null && fieldDeclaration.IsStatic())
                || (propertyDeclaration != null && propertyDeclaration.IsStatic()))
                return;

            var (_, fieldVariableDeclaration) = await context.FindSyntaxForCurrentSpan<VariableDeclaratorSyntax>();
            if (fieldDeclaration != null && fieldVariableDeclaration == null)
            {
                // Verify if it is not a comma or semicolon token.
                var span = context.Span;
                if (span.Start == 0) return;

                var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                var token = root.FindToken(span.Start);
                if (!(token.IsKind(SyntaxKind.CommaToken)
                    || token.IsKind(SyntaxKind.SemicolonToken))) return;

                var movedToken = token.GetPreviousToken();
                if (movedToken == default) return;

                fieldVariableDeclaration = movedToken.Parent.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
                if (fieldVariableDeclaration == null) return;
            }

            switch (nonTrivialConstructorCandidate)
            {
                case ConstructorDeclarationSyntax candidate:
                    {
                        var cancellationToken = context.CancellationToken;
                        var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                        HandleCandidate(semanticModel, candidate, propertyDeclaration, fieldDeclaration, fieldVariableDeclaration);

                    }
                    break;

                case null:
                    {
                        var trivialConstructor = ClassDeclarationSyntaxAnalysis.GetConstructors(classDeclarationSyntax)?.FirstOrDefault();
                        if (trivialConstructor != null)
                        {
                            var cancellationToken = context.CancellationToken;
                            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

                            HandleCandidate(semanticModel, trivialConstructor, propertyDeclaration, fieldDeclaration, fieldVariableDeclaration);
                        }
                        else
                        {
                            // TODO: register add constructor refactoring
                        }
                    }

                    break;

                default:
                    break;
            }

            void HandleCandidate(
                SemanticModel model,
                ConstructorDeclarationSyntax candidate,
                PropertyDeclarationSyntax pDeclaration,
                FieldDeclarationSyntax fDeclaration,
                VariableDeclaratorSyntax variableDeclarator)
            {
                bool isProp = pDeclaration != null;
                var constructorSymbol = model.GetDeclaredSymbol(candidate) as IMethodSymbol;
                ISymbol memberSymbol = isProp ?
                    model.GetDeclaredSymbol(pDeclaration)
                    :  model.GetDeclaredSymbol(variableDeclarator);

                if (constructorSymbol == null || memberSymbol == null) return;

                var analyser = new ConstructorPropertyRelationshipAnalyser(
                    isProp ? Array.Empty<IFieldSymbol>() : new[] { memberSymbol as IFieldSymbol },
                    isProp ? new[] { memberSymbol as IPropertySymbol } : Array.Empty<IPropertySymbol>());

                var result = analyser.Analyze(model, candidate);
                var assignmentResult = result.GetResult(memberSymbol);
                switch (assignmentResult)
                {
                    case AssignmentExpressionAnalyserResult r:
                        // register remove as assignment exists
                        break;

                    case EmptyAssignmentAnalyserResult _:
                        // register add to constructor
                        AnalysedDeclaration ad = isProp ?
                            (AnalysedDeclaration)new PropertyDeclaration(memberSymbol as IPropertySymbol, pDeclaration)
                            : new FieldDeclaration(memberSymbol as IFieldSymbol, fDeclaration, variableDeclarator);
                        context.RegisterRefactoring(
                            new DelegateCodeAction(
                                "Add to constructor",
                                (c) => AddParameterToConstructor(
                                    document,
                                    classDeclarationSyntax,
                                    ad,
                                    constructorSymbol,
                                    candidate,
                                    c)));
                        break;

                    case MultipleAssignments _:
                    case ParsingError _:
                    default:
                        // Something went wrong so it might be better to do nothing.
                        break;
                }
            }
        }

        private static async Task<Document> AddParameterToConstructor(
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

            // TODO: how to determine where to insert assignment ?
            // TODO: use this in assignment ?

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
                var newBody = body.InsertAfter(closestStatement,statementToAdd);
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
            where  U : ISymbol
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

        private abstract class AnalysedDeclaration
        {
            public abstract ISymbol Symbol { get; }
            public abstract TypeSyntax Type { get; }
            public abstract SyntaxToken Identifier { get; }
        }

        private sealed class PropertyDeclaration : AnalysedDeclaration
        {
            IPropertySymbol Property { get; }
            PropertyDeclarationSyntax Declaration { get; }

            public override ISymbol Symbol => Property;
            public override TypeSyntax Type => Declaration.Type;
            public override SyntaxToken Identifier => Declaration.Identifier;

            public PropertyDeclaration(IPropertySymbol property, PropertyDeclarationSyntax declaration)
            {
                Property = property;
                Declaration = declaration;
            }
        }

        private sealed class FieldDeclaration : AnalysedDeclaration
        {
            IFieldSymbol Field { get; }
            FieldDeclarationSyntax FullField { get; }
            VariableDeclaratorSyntax Variable { get; }

            public override ISymbol Symbol => Field;
            public override TypeSyntax Type => FullField.Declaration.Type;
            public override SyntaxToken Identifier => Variable.Identifier;

            public FieldDeclaration(IFieldSymbol field, FieldDeclarationSyntax fullField, VariableDeclaratorSyntax variable)
            {
                Field = field;
                FullField = fullField;
                Variable = variable;
            }
        }
    }
}