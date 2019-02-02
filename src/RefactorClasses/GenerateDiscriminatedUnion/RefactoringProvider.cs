using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
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

namespace RefactorClasses.GenerateDiscriminatedUnion
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "GenerateDiscriminatedUnion"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, classDeclarationSyntax) = await context.GetClassIfOnClassIdentifier();
            if (classDeclarationSyntax == null) return;

            if (!ClassDeclarationSyntaxAnalysis.CanBeDiscriminatedUnionBaseType(classDeclarationSyntax))
                return;

            if (classDeclarationSyntax.Parent == null
                || (!classDeclarationSyntax.Parent.IsKind(SyntaxKind.NamespaceDeclaration)
                    && !classDeclarationSyntax.Parent.IsKind(SyntaxKind.CompilationUnit)))
                return;

            // Find all static methods
            // Convert each of them into type:
            // - parameters into properties
            // - generate constructor from properties - or empty if no properties
            // - generate Equals and Hashcode ?
            // - it is probably safer to regenerate everytime, but what if the
            //   classes implement an abstract member or interface ?
            // - keep non standard overrides, explicit interface implementations ?
            // - or detect abstract members or members of interface (properties only)?
            // - update call to constructor in current class

            var duMembers = GetCandidateMethods(classDeclarationSyntax);
            if (duMembers.Count == 0) return;

            context.RegisterRefactoring(
                new DelegateCodeAction(
                    "Generate discriminated union from static methods",
                    (c) => GenerateDiscriminatedUnion(document, classDeclarationSyntax, c)));
        }

        private static async Task<Document> GenerateDiscriminatedUnion(
            Document document,
            ClassDeclarationSyntax classDeclarationSyntax,
            CancellationToken cancellationToken)
        {
            var duMembers = GetCandidateMethods(classDeclarationSyntax);

            // TODO: Methods returning types defined in other assemblies should probably be rejected,
            // but lets say it is up to user to use this refactoring wisely.
            List<ClassDeclarationSyntax> classesToAdd = new List<ClassDeclarationSyntax>(duMembers.Count);
            foreach (var duCandidate in duMembers)
            {
                var returnIdentifier = duCandidate.ReturnType as IdentifierNameSyntax;
                if (returnIdentifier == null) return document;

                var className = returnIdentifier.Identifier;

                // parameter -> to property
                // to argument -> in constructor call
                var properties = duCandidate.ParameterList.Parameters.Select(ToProperty).ToList();
                var constructorDeclaration = ConstructorGenerationHelper.FromPropertiesWithAssignments(
                    className,
                    properties);
                var members = new List<MemberDeclarationSyntax>(properties);
                members.Add(constructorDeclaration);

                classesToAdd.Add(CreateClass(className, members).WithAdditionalAnnotations(Formatter.Annotation));
            }

            // TODO: replace old definitions if needed ?
            // TODO: Generate method call
            SyntaxNode newNode = null;
            if (classDeclarationSyntax.Parent is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                newNode = namespaceDeclaration.AddMembers(classesToAdd.ToArray());
            }
            else if (classDeclarationSyntax.Parent is CompilationUnitSyntax compilationUnit)
            {
                newNode = compilationUnit.AddMembers(classesToAdd.ToArray());
            }
            else
            {
                return document;
            }

            // Update document
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(classDeclarationSyntax.Parent, newNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static PropertyDeclarationSyntax ToProperty(ParameterSyntax parameter) =>
            SyntaxFactory.PropertyDeclaration(
                parameter.Type,
                SyntaxHelpers.UppercaseIdentifierFirstLetter(parameter.Identifier))
            .WithModifiers(SyntaxFactory.TokenList(Tokens.Public))
            .WithAccessorList(
                SyntaxFactory.AccessorList(
                    Tokens.OpenBrace,
                    SyntaxHelpers.List(
                        SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithLeadingTrivia(SyntaxFactory.Whitespace(" "))
                            .WithTrailingTrivia(SyntaxFactory.Whitespace(" "))
                            .WithSemicolonToken(Tokens.Semicolon)),
                    Tokens.CloseBrace));

        private static ClassDeclarationSyntax CreateClass(
            SyntaxToken identifier,
            IEnumerable<MemberDeclarationSyntax> members) =>
            SyntaxFactory.ClassDeclaration(identifier)
                .WithModifiers(SyntaxFactory.TokenList(Tokens.Public, Tokens.Sealed))
                .WithMembers(SyntaxFactory.List(members));

        private static List<MethodDeclarationSyntax> GetCandidateMethods(ClassDeclarationSyntax classDeclarationSyntax) =>
            ClassDeclarationSyntaxAnalysis.GetMembers<MethodDeclarationSyntax>(classDeclarationSyntax)
                .Where(m => m.IsStatic() && !m.ReturnsPredefinedType())
                .ToList();
    }
}
