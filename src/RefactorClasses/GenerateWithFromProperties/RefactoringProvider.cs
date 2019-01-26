using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.GenerateWithFromProperties
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GenerateWithMethodsFromProperties"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, classDeclarationSyntax) = await context.FindSyntaxForCurrentSpan<ClassDeclarationSyntax>();
            if (document == null || classDeclarationSyntax == null) return;

            if (!ClassDeclarationSyntaxAnalysis.IsRecordLike(classDeclarationSyntax))
            {
                return;
            }

            var properties = ClassDeclarationSyntaxAnalysis.GetPropertyDeclarations(classDeclarationSyntax).ToList();
            if (properties.Count == 0
                || properties.Any(PropertyDeclarationSyntaxExtensions.IsStatic))
                return;

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate with methods for parameters",
                    (c) => GenerateWithMethods(document, classDeclarationSyntax, properties, c)));

            return;
        }

        private static async Task<Document> GenerateWithMethods(
            Document document,
            ClassDeclarationSyntax classDeclarationSyntax,
            IList<PropertyDeclarationSyntax> properties,
            CancellationToken cancellationToken)
        {
            T ExecuteWithTempArg<T>(ArgumentSyntax[] args, int argIdx, ArgumentSyntax tempArg, Func<ArgumentSyntax[], T> operation)
            {
                var oldArg = args[argIdx];
                args[argIdx] = tempArg;

                var result = operation(args);

                args[argIdx] = oldArg;
                return result;
            }

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (properties.Count == 0) return document;

            var classType = SyntaxFactory.IdentifierName(classDeclarationSyntax.Identifier.WithoutTrivia());
            var newClassDeclaration = classDeclarationSyntax;

            var argumentsArray = properties.Select(p => p.ToArgument()).ToArray();

            for (int i = 0; i < properties.Count; ++i)
            {
                var p = properties[i];

                var id = p.Identifier.WithoutTrivia();
                var lcId = SyntaxHelpers.LowercaseIdentifierFirstLetter(id);

                var methodName = WithRefactoringUtils.MethodName(p.Identifier);

                // TODO: Group with members next to each other rather than adding at the end
                var arg = SyntaxHelpers.ArgumentFromIdentifier(lcId);
                var objectCreation = ExecuteWithTempArg(argumentsArray, i, arg, args => ExpressionGenerationHelper.Create(classType, args));

                var withMethodExpression = MethodGenerationHelper.Builder(methodName)
                    .Modifiers(Modifiers.Public)
                    .Parameters(SyntaxHelpers.Parameter(p.Type, lcId))
                    .ReturnType(SyntaxFactory.IdentifierName(classDeclarationSyntax.Identifier.WithoutTrivia()))
                    .ArrowBody(ExpressionGenerationHelper.Arrow(objectCreation))
                    .Build();

                var previousWith = ClassDeclarationSyntaxAnalysis.GetMembers<MethodDeclarationSyntax>(
                    classDeclarationSyntax)
                    .FirstOrDefault(m => m.Identifier.ValueText.Equals(methodName));

                if (previousWith == null)
                {
                    newClassDeclaration = newClassDeclaration.AddMembers(withMethodExpression);
                }
                else
                {
                    newClassDeclaration = newClassDeclaration.ReplaceNode(previousWith, withMethodExpression);
                }
            }

            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(classDeclarationSyntax, newClassDeclaration);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
