using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.GenerateWithFromProperties
{
    internal static class WithRefactoringUtils
    {
        public static string MethodName(SyntaxToken identifier) => $"With{identifier.WithoutTrivia().ValueText}";
    }
}
