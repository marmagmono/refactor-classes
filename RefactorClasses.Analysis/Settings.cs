using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace RefactorClasses.Analysis
{
    internal static class Settings
    {
        // TODO: CarriageReturnLineFeed vs CarriageReturn
        public static SyntaxTrivia EndOfLine => SyntaxFactory.CarriageReturnLineFeed;

        // TODO: read of editor config ?
        public static string IndentOneLevel => "    ";
    }
}
