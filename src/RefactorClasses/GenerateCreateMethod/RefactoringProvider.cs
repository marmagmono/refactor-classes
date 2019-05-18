using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.GenerateCreateMethod
{
    using SH = SyntaxHelpers;

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GenerateCreateMethod"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        private const string CreateMethodName = "Create";

        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, constructorDeclarationSyntax) = await context.FindSyntaxForCurrentSpan<ConstructorDeclarationSyntax>();
            if (document == null || constructorDeclarationSyntax == null) return;

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate create method using constructor",
                    (c) => GenerateCreateMethod(document, constructorDeclarationSyntax, c)));

            return;
        }

        private static async Task<Document> GenerateCreateMethod(
            Document document,
            ConstructorDeclarationSyntax constructor,
            CancellationToken cancellationToken)
        {
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            var classDeclaration = constructor
                .Parent.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (classDeclaration == null) return document;

            var classType = SyntaxFactory.IdentifierName(
                classDeclaration.Identifier.WithoutTrivia());

            var createObjectExpression =
                ExpressionGenerationHelper.CreateObject(
                    classType,
                    constructor.ParameterList.ToArgumentArray());

            var createMethodExpression = MethodGenerationHelper.Builder(CreateMethodName)
                .Modifiers(Modifiers.Public, Modifiers.Static)
                .Parameters(constructor.ParameterList.Parameters.ToArray())
                .ReturnType(SyntaxFactory.IdentifierName(classDeclaration.Identifier.WithoutTrivia()))
                .ArrowBody(ExpressionGenerationHelper.Arrow(createObjectExpression))
                .Build();

            // TODO: validate argument types as well ?
            var previousCreate =
                ClassDeclarationSyntaxAnalysis.GetMembers<MethodDeclarationSyntax>(classDeclaration)
                    .FirstOrDefault(m =>
                        m.Identifier.ValueText.Equals(CreateMethodName)
                        && m.ParameterList.Parameters.Count == createMethodExpression.ParameterList.Parameters.Count);

            // TODO: align with "with" method generation
            var newClassDeclaration = classDeclaration;
            if (previousCreate == null)
            {
                createMethodExpression = createMethodExpression
                    .NormalizeWhitespace(elasticTrivia: false)
                    .WithLeadingTrivia(
                        Settings.EndOfLine,
                        SyntaxFactory.ElasticSpace)
                    .WithTrailingTrivia(Settings.EndOfLine);
                newClassDeclaration = newClassDeclaration.InsertNodesAfter(constructor, new[] { createMethodExpression });
            }
            else
            {
                createMethodExpression = createMethodExpression
                    .NormalizeWhitespace(elasticTrivia: false)
                    .WithTriviaFrom(previousCreate);

                newClassDeclaration = newClassDeclaration.ReplaceNode(previousCreate, createMethodExpression);
            }

            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(classDeclaration, newClassDeclaration);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
