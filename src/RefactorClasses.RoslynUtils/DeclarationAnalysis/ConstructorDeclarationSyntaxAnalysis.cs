using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class ConstructorDeclarationSyntaxAnalysis
    {
        public static bool IsEmpty(this ConstructorDeclarationSyntax constructorDeclaration) =>
            constructorDeclaration.ParameterList.ChildNodes().Count() == 0;
    }
}
