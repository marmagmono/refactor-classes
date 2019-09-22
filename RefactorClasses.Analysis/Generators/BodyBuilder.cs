using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;
    using EGH = ExpressionGenerationHelper;

    public class BodyBuilder
    {
        private List<ExpressionStatementSyntax> expressions = new List<ExpressionStatementSyntax>();

        public BodyBuilder AddAssignment(SyntaxToken left, SyntaxToken right)
        {
            this.expressions.Add(
                SF.ExpressionStatement(
                    EGH.SimpleAssignment(left, right)));
            return this;
        }

        public BodyBuilder AddAssignment(ExpressionSyntax left, ExpressionSyntax right)
        {
            this.expressions.Add(
                SF.ExpressionStatement(
                    EGH.SimpleAssignment(left, right)));
            return this;
        }

        public BlockSyntax Build() => SF.Block(expressions);
    }
}
