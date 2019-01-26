using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class PropertyDeclarationSyntaxExtensions
    {
        public static bool IsStatic(this PropertyDeclarationSyntax propertyDeclaration) =>
            propertyDeclaration.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword);

        public static string NameString(this PropertyDeclarationSyntax propertyDeclaration) =>
            propertyDeclaration.Identifier.ValueText;
    }
}
