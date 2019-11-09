using System.Collections.Generic;
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
            AddExpression(EGH.SimpleAssignment(left, right));
            return this;
        }

        public BodyBuilder AddAssignment(ExpressionSyntax left, ExpressionSyntax right)
        {
            AddExpression(EGH.SimpleAssignment(left, right));
            return this;
        }

        public BodyBuilder AddFieldAssignment(
            IdentifierNameSyntax fieldName,
            ExpressionSyntax rightSide)
        {
            // TODO: configurable this ?
            AddAssignment(
                EGH.ThisMemberAccess(fieldName),
                rightSide);
            return this;
        }

        public BodyBuilder AddVoidMemberInvocation(
            IdentifierNameSyntax objectName,
            IdentifierNameSyntax methodName,
            params ArgumentSyntax[] arguments)
        {
            var methodAccess = SF.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                objectName, methodName);

            AddExpression(
                SF.InvocationExpression(
                    methodAccess,
                    ToArgList(arguments)));

            return this;
        }

        public BodyBuilder AddVoidInvocation(
            IdentifierNameSyntax identifier,
            params ArgumentSyntax[] arguments)
        {
            AddExpression(EGH.Invocation(identifier, arguments));
            return this;
        }

        public BlockSyntax Build() => SF.Block(expressions);

        private void AddExpression(ExpressionSyntax expression) =>
            this.expressions.Add(
                SF.ExpressionStatement(expression));

        private static ArgumentListSyntax ToArgList(params ArgumentSyntax[] arguments) =>
            SF.ArgumentList(SF.SeparatedList(arguments));
    }
}
