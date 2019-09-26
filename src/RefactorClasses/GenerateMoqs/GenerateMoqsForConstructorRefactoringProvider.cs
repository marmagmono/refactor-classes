using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.DeclarationGeneration;
using RefactorClasses.Analysis.Generators;
using RefactorClasses.Analysis.Inspections.Method;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;

namespace RefactorClasses.GenerateMoqs
{
    using SF = SyntaxFactory;
    using GH = GeneratorHelper;
    using EGH = ExpressionGenerationHelper;

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GenerateClassFromMethod"), Shared]
    public sealed class GenerateMoqsForConstructorRefactoringProvider: CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, methodDeclarationSyntax) =
                    await context.FindSyntaxForCurrentSpan<ConstructorDeclarationSyntax>();

            if (document == null || methodDeclarationSyntax == null) return;

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate test with moq",
                    (c) => GenerateClassFromMethod(document, methodDeclarationSyntax, c)));
        }

        private static async Task<Document> GenerateClassFromMethod(
            Document document,
            ConstructorDeclarationSyntax constructor,
            CancellationToken cancellationToken)
        {
            var parentClass = constructor.Parent.FirstAncestorOrSelf<ClassDeclarationSyntax>();
            if (parentClass == null) return document;

            var classDeclaration = CreateClass(constructor);

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.InsertNodesAfter(parentClass, new[] { classDeclaration });
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static ClassDeclarationSyntax CreateClass(
            ConstructorDeclarationSyntax constructor)
        {
            ObjectCreationExpressionSyntax CreateMoq(TypeSyntax moqType) =>
                EGH.CreateObject(
                    moqType,
                    SF.Argument(
                        EGH.MemberAccess(
                            GH.Identifier("MockBehavior"),
                            GH.Identifier("Strict"))));

            var mi = new MethodInspector(constructor);

            var recordBuilder = new RecordBuilder($"{mi.Name}Test")
                    .AddModifiers(Modifiers.Public);

            foreach (var p in mi.Parameters)
            {
                var moqType = GH.GenericName("Mock", p.Type);
                recordBuilder.AddField(moqType, $"{p.Name}Mock", CreateMoq(moqType));
            }

            return recordBuilder.Build();
        }
    }
}
