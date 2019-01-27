using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    internal static class ThrowHelpers
    {
        public static void ThrowIfNotIdentifier(string parameterName, SyntaxToken parameter)
        {
            if (!parameter.IsKind(SyntaxKind.IdentifierToken))
            {
                throw new ArgumentException($"{parameterName} is expected to be of {SyntaxKind.IdentifierToken} kind");
            }
        }
    }
}
