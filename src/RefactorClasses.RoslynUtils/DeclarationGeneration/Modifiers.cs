using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class Modifiers
    {
        public static SyntaxToken Public => SyntaxFactory.Token(SyntaxKind.PublicKeyword);

        public static SyntaxToken Internal => SyntaxFactory.Token(SyntaxKind.InternalKeyword);

        public static SyntaxToken Protected => SyntaxFactory.Token(SyntaxKind.ProtectedKeyword);

        public static SyntaxToken Private => SyntaxFactory.Token(SyntaxKind.PrivateKeyword);

        public static SyntaxToken Override => SyntaxFactory.Token(SyntaxKind.OverrideKeyword);

        public static SyntaxToken Virtual => SyntaxFactory.Token(SyntaxKind.VirtualKeyword);

        public static SyntaxToken Sealed => SyntaxFactory.Token(SyntaxKind.SealedKeyword);

        public static SyntaxToken Static => SyntaxFactory.Token(SyntaxKind.StaticKeyword);

        public static SyntaxToken Abstract => SyntaxFactory.Token(SyntaxKind.AbstractKeyword);
    }
}
