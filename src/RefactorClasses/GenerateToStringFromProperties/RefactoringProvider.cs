using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
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

namespace ClassAnalyzer
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GenerateToStringFromProperties"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        private const string ToStringMethodName = "ToString";

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
                    "Generate to string from parameters",
                    (c) => GenerateToString(document, classDeclarationSyntax, properties, c)));

            return;
        }

        private static async Task<Document> GenerateToString(
            Document document,
            ClassDeclarationSyntax classDeclarationSyntax,
            IList<PropertyDeclarationSyntax> properties,
            CancellationToken cancellationToken)
        {
            InvocationExpressionSyntax GenerateNameOfCall() =>
                ExpressionGenerationHelper.Invocation(
                    "nameof",
                    SyntaxHelpers.ArgumentFromIdentifier(classDeclarationSyntax.Identifier));

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

            if (properties.Count == 0) return document;

            var firstEl = Enumerable.Repeat(
                (InterpolatedStringContentSyntax)SyntaxFactory.Interpolation(GenerateNameOfCall()),
                1);
            var interpolatedStringExpression = InterpolatedStringGenerationHelper.InterpolatedString(
                firstEl.Concat(
                    properties.SelectMany(p =>
                        new InterpolatedStringContentSyntax[]
                        {
                            InterpolatedStringGenerationHelper.Text($" {p.Identifier.WithoutTrivia().ValueText}="),
                            SyntaxFactory.Interpolation(SyntaxFactory.IdentifierName(p.Identifier.WithoutTrivia()))
                        })));

            var methodExpression = MethodGenerationHelper.Builder(ToStringMethodName)
                .Modifiers(Modifiers.Public, Modifiers.Override)
                .ReturnType(Types.String)
                .ArrowBody(ExpressionGenerationHelper.Arrow(interpolatedStringExpression))
                .Build();

            var previousToString = ClassDeclarationSyntaxAnalysis.GetMembers<MethodDeclarationSyntax>(
                classDeclarationSyntax)
                .FirstOrDefault(m => m.Identifier.ValueText.Equals(ToStringMethodName));

            var newClassDeclaration =
                previousToString != null ?
                classDeclarationSyntax.ReplaceNode(previousToString, methodExpression)
                : classDeclarationSyntax.AddMembers(methodExpression);

            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(classDeclarationSyntax, newClassDeclaration);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
