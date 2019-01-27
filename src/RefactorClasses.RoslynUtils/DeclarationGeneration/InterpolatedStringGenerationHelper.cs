using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class InterpolatedStringGenerationHelper
    {
        public static InterpolatedStringExpressionSyntax InterpolatedString(
            params InterpolatedStringContentSyntax[] content) =>
            InterpolatedString(content as IEnumerable<InterpolatedStringContentSyntax>);

        public static InterpolatedStringExpressionSyntax InterpolatedString(
            IEnumerable<InterpolatedStringContentSyntax> content) =>
            SyntaxFactory.InterpolatedStringExpression(
                SyntaxFactory.Token(SyntaxKind.InterpolatedStringStartToken),
                SyntaxFactory.List(content),
                SyntaxFactory.Token(SyntaxKind.InterpolatedStringEndToken));

        public static InterpolatedStringTextSyntax Text(SyntaxToken token)
        {
            if (token.IsKind(SyntaxKind.InterpolatedStringTextToken))
                return SyntaxFactory.InterpolatedStringText(token);
            else
                return Text(token.ValueText);
        }

        public static InterpolatedStringTextSyntax Text(string text) =>
            SyntaxFactory.InterpolatedStringText(
                SyntaxFactory.Token(
                    SyntaxFactory.TriviaList(),
                    SyntaxKind.InterpolatedStringTextToken,
                    text,
                    text,
                    SyntaxFactory.TriviaList()));
    }
}
