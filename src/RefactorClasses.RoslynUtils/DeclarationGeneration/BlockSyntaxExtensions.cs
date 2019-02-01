using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class BlockSyntaxExtensions
    {
        public static BlockSyntax InsertBefore(
            this BlockSyntax blockSyntax,
            ExpressionStatementSyntax statement,
            ExpressionStatementSyntax statementToInsert)
        {
            var idx = blockSyntax.Statements.IndexOf(statement);
            if (idx == -1)
            {
                return blockSyntax;
            }
            else
            {
                return blockSyntax.WithStatements(
                    blockSyntax.Statements.Insert(idx, statementToInsert));
            }
        }

        public static BlockSyntax InsertAfter(
            this BlockSyntax blockSyntax,
            ExpressionStatementSyntax statement,
            ExpressionStatementSyntax statementToInsert)
        {
            var idx = blockSyntax.Statements.IndexOf(statement);
            if (idx == -1)
            {
                return blockSyntax;
            }
            else if (idx == blockSyntax.Statements.Count - 1)
            {
                return blockSyntax.AddStatements(statementToInsert);
            }
            else
            {
                return blockSyntax.WithStatements(
                    blockSyntax.Statements.Insert(idx + 1, statementToInsert));
            }
        }
    }
}
