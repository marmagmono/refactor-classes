using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors;
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

            var (atMostOneConstructor, nonTrivialConstructorCandidate) =
                ClassDeclarationSyntaxAnalysis.HasAtMostOneNoneTrivialConstructor(classDeclarationSyntax);

            if (!atMostOneConstructor || nonTrivialConstructorCandidate == null) return;

            var properties = ClassDeclarationSyntaxAnalysis.GetPropertyDeclarations(classDeclarationSyntax).ToList();
            if (properties.Count == 0
                || properties.Any(PropertyDeclarationSyntaxExtensions.IsStatic))
                return;

            var cancellationToken = context.CancellationToken;
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);

            var constructorSymbol = semanticModel.GetDeclaredSymbol(nonTrivialConstructorCandidate);
            var propertySymbols = properties.Select(p => semanticModel.GetDeclaredSymbol(p, cancellationToken)).ToArray();

            var analyser = new ConstructorPropertyRelationshipAnalyser(
                Array.Empty<IFieldSymbol>(),
                propertySymbols);

            var result = analyser.Analyze(semanticModel, nonTrivialConstructorCandidate);
            var (idxMappings, isExhaustive) = result.GetIndexMapping(propertySymbols, constructorSymbol);
            if (!isExhaustive)
            {
                return;
            }

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate with methods for parameters",
                    (c) => GenerateWithMethods(document, classDeclarationSyntax, properties, idxMappings, c)));

            return;
        }

        private static async Task<Document> GenerateWithMethods(
            Document document,
            ClassDeclarationSyntax classDeclarationSyntax,
            IList<PropertyDeclarationSyntax> properties,
            int[] propertyToParameterIdx,
            CancellationToken cancellationToken)
        {
            int MappedIdx(int i)
            {
                int mappedIdx = propertyToParameterIdx[i];
                if (mappedIdx == -1) throw new ArgumentException($"{nameof(propertyToParameterIdx)} contains invalid mapping");
                return mappedIdx;
            }

            ArgumentSyntax[] ReorderArgumentsToMapping(IReadOnlyList<ArgumentSyntax> arguments)
            {
                var result = new ArgumentSyntax[arguments.Count];
                for (int i = 0; i < arguments.Count; ++i)
                {
                    result[MappedIdx(i)] = arguments[i];
                }

                return result;
            }

            T ExecuteWithTempArg<T>(ArgumentSyntax[] args, int argIdx, ArgumentSyntax tempArg, Func<ArgumentSyntax[], T> operation)
            {
                var oldArg = args[argIdx];
                args[argIdx] = tempArg;

                var result = operation(args);

                args[argIdx] = oldArg;
                return result;
            }

            if (properties.Count != propertyToParameterIdx.Length)
                throw new ArgumentException("properties.Count != propertyToParameterIdx.Length");

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (properties.Count == 0) return document;

            var classType = SyntaxFactory.IdentifierName(classDeclarationSyntax.Identifier.WithoutTrivia());
            var newClassDeclaration = classDeclarationSyntax;

            var argumentsArray = properties.Select(p => p.ToArgument()).ToArray();
            argumentsArray = ReorderArgumentsToMapping(argumentsArray);

            for (int i = 0; i < properties.Count; ++i)
            {
                var p = properties[i];
                var id = p.Identifier.WithoutTrivia();
                var lcId = SyntaxHelpers.LowercaseIdentifierFirstLetter(id);
                var methodName = WithRefactoringUtils.MethodName(p.Identifier);
                var arg = SyntaxHelpers.ArgumentFromIdentifier(lcId);

                var objectCreation = ExecuteWithTempArg(argumentsArray, MappedIdx(i), arg, args => ExpressionGenerationHelper.CreateObject(classType, args));

                var withMethodExpression = MethodGenerationHelper.Builder(methodName)
                    .Modifiers(Modifiers.Public)
                    .Parameters(SyntaxHelpers.Parameter(p.Type, lcId))
                    .ReturnType(SyntaxFactory.IdentifierName(classDeclarationSyntax.Identifier.WithoutTrivia()))
                    .ArrowBody(ExpressionGenerationHelper.Arrow(objectCreation))
                    .Build();

                var previousWith = ClassDeclarationSyntaxAnalysis.GetMembers<MethodDeclarationSyntax>(
                    newClassDeclaration)
                    .FirstOrDefault(m => m.Identifier.ValueText.Equals(methodName));

                if (previousWith == null)
                {
                    // TODO: Group with members next to each other rather than adding at the end
                    withMethodExpression = withMethodExpression
                        .NormalizeWhitespace(elasticTrivia: false)
                        .WithLeadingTrivia(
                            Settings.EndOfLine,
                            SyntaxFactory.ElasticSpace)
                        .WithTrailingTrivia(Settings.EndOfLine);

                    newClassDeclaration = newClassDeclaration.AddMembers(withMethodExpression);
                }
                else
                {
                    withMethodExpression = withMethodExpression
                        .NormalizeWhitespace(elasticTrivia: false)
                        .WithTriviaFrom(previousWith);

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
