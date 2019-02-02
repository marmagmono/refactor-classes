using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors;
using System;
using System.Composition;
using System.Linq;
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

            var (_, fieldVariableDeclaration) = await context.FindVariableDeclaratorForCurrentSpan();
            if (fieldDeclaration != null && fieldVariableDeclaration == null) return;

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
                AnalysedDeclaration ad = isProp ?
                    (AnalysedDeclaration)new PropertyDeclaration(memberSymbol as IPropertySymbol, pDeclaration)
                    : new FieldDeclaration(memberSymbol as IFieldSymbol, fDeclaration, variableDeclarator);

                switch (assignmentResult)
                {
                    case AssignmentExpressionAnalyserResult r:
                        // register remove as assignment exists
                        context.RegisterRefactoring(
                            new DelegateCodeAction(
                                $"Remove {ad.Identifier.ValueText}",
                                (c) => RefactoringActions.RemoveParameter(
                                    document,
                                    classDeclarationSyntax,
                                    ad,
                                    candidate,
                                    c)));
                        break;

                    case EmptyAssignmentAnalyserResult _:
                        // register add to constructor
                        context.RegisterRefactoring(
                            new DelegateCodeAction(
                                $"Add {ad.Identifier.ValueText} to constructor",
                                (c) => RefactoringActions.AddParameterToConstructor(
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
    }
}