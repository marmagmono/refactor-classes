using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class Tokens
    {
        public static SyntaxToken Return => SyntaxFactory.Token(SyntaxKind.ReturnKeyword);

        public static SyntaxToken Semicolon => SyntaxFactory.Token(SyntaxKind.SemicolonToken);
    }
}
