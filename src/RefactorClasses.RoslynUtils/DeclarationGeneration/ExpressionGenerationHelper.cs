using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class ExpressionGenerationHelper
    {
        public static AssignmentExpressionSyntax SimpleAssignment(
            SyntaxToken leftIdentifier,
            SyntaxToken rightIdentifier)
        {
            ThrowHelpers.ThrowIfNotIdentifier(nameof(leftIdentifier), leftIdentifier);
            ThrowHelpers.ThrowIfNotIdentifier(nameof(rightIdentifier), rightIdentifier);

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

        private static ArgumentListSyntax ToArgList(params ArgumentSyntax[] arguments) =>
            SyntaxFactory.ArgumentList(SyntaxHelpers.SeparatedSyntaxList(arguments));
    }
}
