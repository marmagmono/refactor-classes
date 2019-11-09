using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Test
{
    public static class TestHelpers
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemCoreReference = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        //private static readonly MetadataReference CSharpSymbolsReference = MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        //private static readonly MetadataReference CodeAnalysisReference = MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);

        private const string TestCompilationName = "TestProject";

        public static Compilation CreateCompilation(SyntaxTree syntaxTree) =>
            CSharpCompilation
                .Create(TestCompilationName)
                .AddReferences(CorlibReference, SystemCoreReference)
                .AddSyntaxTrees(syntaxTree);

        public static ClassDeclarationSyntax FindFirstClassDeclaration(SyntaxNode node)
        {
            if (node is CompilationUnitSyntax compilationUnit)
            {
                var firstMember = compilationUnit.Members.FirstOrDefault();
                if (firstMember == null) throw new ArgumentException("Compilation unit is empty");

                if (firstMember is NamespaceDeclarationSyntax nds)
                {
                    var firstNamespaceElement = nds.Members.FirstOrDefault();
                    if (firstNamespaceElement == null) throw new ArgumentException("Namespace is empty");

                    if (firstNamespaceElement is ClassDeclarationSyntax cds)
                    {
                        return cds;
                    }

                    throw new ArgumentException("Namespace does not contain class");
                }
            }

            throw new ArgumentException($"Type of {node} is not supported.");
        }
    }
}
