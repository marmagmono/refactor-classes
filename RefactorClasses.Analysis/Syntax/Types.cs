using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Syntax
{
    public static class Types
    {
        public static TypeSyntax Void =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword));

        public static TypeSyntax Int =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

        public static TypeSyntax UInt =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword));

        public static TypeSyntax Long =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword));

        public static TypeSyntax ULong =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword));

        public static TypeSyntax Float =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword));

        public static TypeSyntax Double =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword));

        public static TypeSyntax String =>
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));
    }
}
