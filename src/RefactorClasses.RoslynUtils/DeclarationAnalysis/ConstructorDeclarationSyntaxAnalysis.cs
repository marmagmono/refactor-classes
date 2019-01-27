using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class ConstructorDeclarationSyntaxAnalysis
    {
        public static bool IsEmpty(ConstructorDeclarationSyntax constructorDeclaration) =>
            constructorDeclaration.ParameterList.ChildNodes().Count() == 0;
    }
}
