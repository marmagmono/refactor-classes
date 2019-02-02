using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class FieldDeclarationSyntaxExtensions
    {
        public static bool IsStatic(this FieldDeclarationSyntax fieldDeclaration) =>
            fieldDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public static bool HasMultipleVariables(this FieldDeclarationSyntax fieldDeclaration) =>
            fieldDeclaration.Declaration.Variables.Count > 1;

        public static string FirstNameString(this FieldDeclarationSyntax fieldDeclaration) =>
            fieldDeclaration.Declaration.Variables.FirstOrDefault()?.Identifier.ValueText;
    }
}
