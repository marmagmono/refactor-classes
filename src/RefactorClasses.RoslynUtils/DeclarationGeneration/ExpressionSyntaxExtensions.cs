using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class ExpressionSyntaxExtensions
    {
        public static ExpressionStatementSyntax ToStatement(this ExpressionSyntax es) =>
            SyntaxFactory.ExpressionStatement(es, Tokens.Semicolon);

        public static ReturnStatementSyntax Return(this ExpressionSyntax es) =>
            SyntaxFactory.ReturnStatement(Tokens.Return, es, Tokens.Semicolon);
    }
}
