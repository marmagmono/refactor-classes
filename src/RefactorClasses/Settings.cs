using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses
{
    internal static class Settings
    {
        // TODO: CarriageReturnLineFeed vs CarriageReturn
        public static SyntaxTrivia EndOfLine => SyntaxFactory.CarriageReturnLineFeed;

        // TODO: read of editor config ?
        public static string IndentOneLevel => "    ";
    }
}
