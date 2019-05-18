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
    using SF = SyntaxFactory;
    using SH = SyntaxHelpers;
    using EGH = ExpressionGenerationHelper;

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
            var rootNode = classDeclarationSyntax.Parent;
            if (rootNode == null) return document;

            var duMembers = GetCandidateMethods(classDeclarationSyntax);
            if (duMembers.Count == 0) return document;

            var factoryMethodRewriter = new FactoryMethodRewriter();
            var newClassDeclaration = factoryMethodRewriter.Visit(classDeclarationSyntax);
            rootNode = rootNode.ReplaceNode(classDeclarationSyntax, newClassDeclaration);

            List<(MethodDeclarationSyntax method, ClassDeclarationSyntax cl)> candidates =
                duMembers.Select(m => (m, FindCurrentCaseDeclaration(rootNode, GetGeneratedClassName(m))))
                .ToList();

            var declarationsCleaner = new ClassOccurencesCleaner(candidates.Select(p => p.cl).Where(p => p != null).ToList());
            rootNode = declarationsCleaner.Visit(rootNode);

            // TODO: Methods returning types defined in other assemblies should probably be rejected,
            // but lets say it is user's responsibility to use this refactoring wisely.

            var baseClassIdentifier = SF.IdentifierName(classDeclarationSyntax.Identifier);
            foreach (var (duCandidate, prevDeclaration) in candidates)
            {
                var generatedClassName = GetGeneratedClassName(duCandidate);
                if (generatedClassName == default) return document;

                var properties = duCandidate.ParameterList.Parameters.Select(ToProperty).ToList();
                var constructorDeclaration = properties.Count > 0 ?
                    ConstructorGenerationHelper.FromPropertiesWithAssignments(
                        generatedClassName,
                        properties)
                    : null;
                var members = new List<MemberDeclarationSyntax>(properties);
                rootNode = UpdateOrAddCaseDefinition(rootNode, generatedClassName, baseClassIdentifier, properties, constructorDeclaration, prevDeclaration);
            }

            // Update document
            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(classDeclarationSyntax.Parent, rootNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static SyntaxNode UpdateOrAddCaseDefinition(
            SyntaxNode namespaceNode,
            SyntaxToken className,
            IdentifierNameSyntax baseClassIdentifier,
            List<PropertyDeclarationSyntax> properties,
            ConstructorDeclarationSyntax constructorDeclaration,
            ClassDeclarationSyntax previousDeclaration)
        {
            var overrideMethods = previousDeclaration != null ?
                previousDeclaration.GetOverrideMethods()
                : Enumerable.Empty<MethodDeclarationSyntax>();
            var overrideProperties = previousDeclaration != null ?
                previousDeclaration.GetOverrideProperties()
                : Enumerable.Empty<PropertyDeclarationSyntax>();

            var members =
                    properties.Cast<MemberDeclarationSyntax>()
                        .Concat(overrideProperties)
                        .Concat(overrideMethods).ToList();
            if (constructorDeclaration != null)
                members.Add(constructorDeclaration);

            return AddMemberNode(
                        namespaceNode,
                        CreateClass(className, baseClassIdentifier, members)
                            .WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static ClassDeclarationSyntax FindCurrentCaseDeclaration(SyntaxNode syntaxNode, SyntaxToken className)
        {
            if (syntaxNode is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                return FindClass(namespaceDeclaration.Members, className);
            }
            else if (syntaxNode is CompilationUnitSyntax compilationUnit)
            {
                return FindClass(compilationUnit.Members, className);
            }
            else
            {
                return null;
            }

            ClassDeclarationSyntax FindClass(SyntaxList<MemberDeclarationSyntax> members, SyntaxToken identifier) =>
                members.FirstOrDefault(m => m.IsKind(SyntaxKind.ClassDeclaration)
                                            && ((ClassDeclarationSyntax)m).Identifier.ValueText.Equals(identifier.ValueText))
                as ClassDeclarationSyntax;
        }

        private static SyntaxNode AddMemberNode(SyntaxNode namespaceNode, MemberDeclarationSyntax member)
        {
            if (namespaceNode is NamespaceDeclarationSyntax namespaceDeclaration)
            {
                return namespaceDeclaration.AddMembers(member);
            }
            else if (namespaceNode is CompilationUnitSyntax compilationUnit)
            {
                return compilationUnit.AddMembers(member);
            }
            else
            {
                return namespaceNode;
            }
        }

        private static SyntaxToken GetGeneratedClassName(MethodDeclarationSyntax methodDeclaration)
        {
            var returnIdentifier = methodDeclaration.ReturnType as IdentifierNameSyntax;
            if (returnIdentifier == null) return default;

            return returnIdentifier.Identifier;
        }

        private static PropertyDeclarationSyntax ToProperty(ParameterSyntax parameter) =>
            SF.PropertyDeclaration(
                parameter.Type,
                SyntaxHelpers.UppercaseIdentifierFirstLetter(parameter.Identifier))
            .WithModifiers(SF.TokenList(Tokens.Public))
            .WithAccessorList(
                SF.AccessorList(
                    Tokens.OpenBrace,
                    SyntaxHelpers.List(
                        SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                            .WithLeadingTrivia(SF.Whitespace(" "))
                            .WithTrailingTrivia(SF.Whitespace(" "))
                            .WithSemicolonToken(Tokens.Semicolon)),
                    Tokens.CloseBrace));

        private static ClassDeclarationSyntax CreateClass(
            SyntaxToken identifier,
            IdentifierNameSyntax baseIndentifier,
            IEnumerable<MemberDeclarationSyntax> members) =>
            SF.ClassDeclaration(identifier)
                .WithBaseList(SH.BaseList(baseIndentifier))
                .WithModifiers(SF.TokenList(Tokens.Public, Tokens.Sealed))
                .WithMembers(SF.List(members));

        private static List<MethodDeclarationSyntax> GetCandidateMethods(ClassDeclarationSyntax classDeclarationSyntax) =>
            ClassDeclarationSyntaxAnalysis.GetMembers<MethodDeclarationSyntax>(classDeclarationSyntax)
                .Where(IsDuCandidateMethod)
                .ToList();

        private static bool IsDuCandidateMethod(MethodDeclarationSyntax m) =>
            m.IsStatic() && !m.ReturnsPredefinedType();

        private class FactoryMethodRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
            {
                if (IsDuCandidateMethod(node))
                {
                    var generatedClassName = GetGeneratedClassName(node);
                    if (generatedClassName == default) return base.VisitMethodDeclaration(node);

                    var createObjectCall = EGH.CreateObject(
                        SF.IdentifierName(generatedClassName),
                        node.ParameterList.ToArgumentArray());

                    return node
                        .WithBody(null)
                        .WithExpressionBody(EGH.Arrow(createObjectCall))
                        .WithSemicolonToken(Tokens.Semicolon.WithTrailingTrivia(
                            SF.TriviaList(Settings.EndOfLine)));
                }

                return base.VisitMethodDeclaration(node);
            }
        }

        private class ClassOccurencesCleaner : CSharpSyntaxRewriter
        {
            private readonly List<ClassDeclarationSyntax> classDeclarationSyntaxes;

            public ClassOccurencesCleaner(List<ClassDeclarationSyntax> classDeclarationSyntaxes)
            {
                this.classDeclarationSyntaxes = classDeclarationSyntaxes;
            }

            public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (classDeclarationSyntaxes.FirstOrDefault(c => c.Equals(node)) != null)
                {
                    return null;
                }

                return base.VisitClassDeclaration(node);
            }
        }
    }
}
