using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    public static class ExpressionGenerationHelper
    {
        public static AssignmentExpressionSyntax SimpleAssignment(
            SyntaxToken leftIdentifier,
            SyntaxToken rightIdentifier)
        {
            return SimpleAssignment(
                SyntaxFactory.IdentifierName(leftIdentifier),
                SyntaxFactory.IdentifierName(rightIdentifier));
        }

        public static AssignmentExpressionSyntax SimpleAssignment(
            IdentifierNameSyntax left,
            IdentifierNameSyntax right) =>
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                left,
                right);

        public static AssignmentExpressionSyntax SimpleAssignment(
            ExpressionSyntax left,
            ExpressionSyntax right) =>
            SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                left,
                right);

        public static InvocationExpressionSyntax Invocation(
            string identifierName,
            params ArgumentSyntax[] arguments) =>
                Invocation(SyntaxFactory.IdentifierName(identifierName), arguments);

        public static InvocationExpressionSyntax Invocation(
            IdentifierNameSyntax identifierName,
            params ArgumentSyntax[] arguments) =>
                SyntaxFactory.InvocationExpression(
                    identifierName, ToArgList(arguments));

        public static ObjectCreationExpressionSyntax CreateObject(
            TypeSyntax type,
            params ArgumentSyntax[] arguments) =>
            SyntaxFactory.ObjectCreationExpression(type, ToArgList(arguments), null);

        public static ArrowExpressionClauseSyntax Arrow(ExpressionSyntax expression) =>
            SyntaxFactory.ArrowExpressionClause(expression);

        public static MemberAccessExpressionSyntax ThisMemberAccess(IdentifierNameSyntax identifierName) =>
            SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                identifierName);

        private static ArgumentListSyntax ToArgList(params ArgumentSyntax[] arguments) =>
            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments));
    }
}
