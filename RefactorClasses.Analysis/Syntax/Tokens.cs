using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactorClasses.Analysis.DeclarationGeneration
{
    public static class Tokens
    {
        public static SyntaxToken Return => SyntaxFactory.Token(SyntaxKind.ReturnKeyword);

        public static SyntaxToken Semicolon => SyntaxFactory.Token(SyntaxKind.SemicolonToken);

        public static SyntaxToken Colon => SyntaxFactory.Token(SyntaxKind.ColonToken);

        public static SyntaxToken Comma => SyntaxFactory.Token(SyntaxKind.CommaToken);

        public static SyntaxToken OpenParen => SyntaxFactory.Token(SyntaxKind.OpenParenToken);

        public static SyntaxToken OpenBrace => SyntaxFactory.Token(SyntaxKind.OpenBraceToken);

        public static SyntaxToken CloseBrace => SyntaxFactory.Token(SyntaxKind.CloseBraceToken);
    }
}
