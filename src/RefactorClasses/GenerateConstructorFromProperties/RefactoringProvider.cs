using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.GenerateConstructorFromProperties
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GeneratePropertyConstructor"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, classDeclarationSyntax) = await context.FindSyntaxForCurrentSpan<ClassDeclarationSyntax>();
            if (document == null || classDeclarationSyntax == null) return;

            if (!ClassDeclarationSyntaxAnalysis.IsRecordLike(classDeclarationSyntax)) return;

            var (atMostOneConstructor, nonTrivialConstructorCandidate) =
                ClassDeclarationSyntaxAnalysis.HasAtMostOneNoneTrivialConstructor(classDeclarationSyntax);

            if (!atMostOneConstructor) return;

            var properties = ClassDeclarationSyntaxAnalysis.GetPropertyDeclarations(classDeclarationSyntax).ToList();
            if (properties.Count == 0
                || properties.Any(PropertyDeclarationSyntaxExtensions.IsStatic))
                return;

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate constructor from parameters",
                    (c) => GenerateConstructor(document, classDeclarationSyntax, properties, nonTrivialConstructorCandidate, c)));

            return;
        }

        private static async Task<Document> GenerateConstructor(
            Document document,
            ClassDeclarationSyntax classDeclarationSyntax,
            IList<PropertyDeclarationSyntax> properties,
            ConstructorDeclarationSyntax nonTrivialConstructorCandidate,
            CancellationToken cancellationToken)
        {
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (properties.Count == 0) return document;

            var constructorDeclaration = ConstructorGenerationHelper.FromPropertiesWithAssignments(
                classDeclarationSyntax.Identifier.WithoutTrivia(),
                properties);

            var newClassDeclaration = nonTrivialConstructorCandidate != null ?
                classDeclarationSyntax.ReplaceNode(nonTrivialConstructorCandidate, constructorDeclaration)
                : classDeclarationSyntax.AddMembers(constructorDeclaration);

            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(classDeclarationSyntax, newClassDeclaration);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
