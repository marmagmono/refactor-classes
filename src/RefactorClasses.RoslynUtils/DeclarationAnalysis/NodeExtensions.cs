using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class NodeExtensions
    {
        private const string EmptyIndent = "";

        public static string EstimateIndent(
            this SyntaxNode node,
            SyntaxTree tree,
            CancellationToken cancellationToken)
        {
            var currentNode = node;
            while (currentNode != null
                && tree.GetLineSpan(currentNode.FullSpan, cancellationToken).StartLinePosition.Character != 0)
            {
                currentNode = currentNode.Parent;
            }

            if (currentNode == null) return EmptyIndent;

            var token = currentNode.FindToken(currentNode.FullSpan.Start);
            if (!token.HasLeadingTrivia)
                return EmptyIndent;

            var lastTrivia = token.LeadingTrivia.Last();
            if (!lastTrivia.IsKind(SyntaxKind.WhitespaceTrivia)) return EmptyIndent;

            // TODO: Handle tabs and more exotic cases of mixed tabs and whitespaces.
            return new string(' ', lastTrivia.FullSpan.Length);
        }
    }
}
