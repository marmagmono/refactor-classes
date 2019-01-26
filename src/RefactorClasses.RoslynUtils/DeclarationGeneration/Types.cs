using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class Types
    {
        public static TypeSyntax Void =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

        internal static TypeSyntax Int =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

        internal static TypeSyntax UInt =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword));

        internal static TypeSyntax Long =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));

        internal static TypeSyntax ULong =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword));

        internal static TypeSyntax Float =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));

        internal static TypeSyntax Double =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));

        internal static TypeSyntax String =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
    }
}
