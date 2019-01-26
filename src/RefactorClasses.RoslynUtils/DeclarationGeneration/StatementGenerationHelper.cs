using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class StatementGenerationHelper
    {
        public static ReturnStatementSyntax Return(SyntaxToken identifierToken) =>
            SyntaxFactory.ReturnStatement(SyntaxFactory.IdentifierName(identifierToken));
    }
}
