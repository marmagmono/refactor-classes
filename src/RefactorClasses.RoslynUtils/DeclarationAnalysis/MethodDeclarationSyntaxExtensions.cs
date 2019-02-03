using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class MethodDeclarationSyntaxExtensions
    {
        public static bool IsStatic(this MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public static bool ReturnsPredefinedType(this MethodDeclarationSyntax methodDeclaration) =>
            methodDeclaration.ReturnType.IsKind(SyntaxKind.PredefinedType);
    }
}
