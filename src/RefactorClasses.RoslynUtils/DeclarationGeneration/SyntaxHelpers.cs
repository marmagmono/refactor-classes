using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class SyntaxHelpers
    {
        public static SyntaxList<AttributeListSyntax> EmptyAttributeList() =>
            SyntaxFactory.List<AttributeListSyntax>();

        public static SyntaxTokenList EmptyModifierList() =>
            SyntaxFactory.TokenList();

        public static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> parameters) =>
            SyntaxFactory.ParameterList(
                SyntaxFactory.Token(SyntaxKind.OpenParenToken),
                SyntaxFactory.SeparatedList(parameters),
                SyntaxFactory.Token(SyntaxKind.CloseParenToken));

        public static SeparatedSyntaxList<TNode> SeparatedSyntaxList<TNode>(params TNode[] nodes) where TNode : SyntaxNode =>
            SyntaxFactory.SeparatedList(nodes);

        public static SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : SyntaxNode =>
            SyntaxFactory.List(nodes);

        public static SyntaxToken Identifier(string name) => SyntaxFactory.Identifier(name);

        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier) =>
            SyntaxFactory.Parameter(
                EmptyAttributeList(),
                EmptyModifierList(),
                type,
                identifier,
                default(EqualsValueClauseSyntax));

        public static ArgumentSyntax ArgumentFromIdentifier(SyntaxToken identifier) =>
            SyntaxFactory.Argument(SyntaxFactory.IdentifierName(identifier.WithoutTrivia()));

        public static SyntaxToken LowercaseIdentifierFirstLetter(SyntaxToken identifier)
        {
            ThrowHelpers.ThrowIfNotIdentifier(nameof(identifier), identifier);

            if (identifier.Value is string s && s.Length >= 1)
            {
                var newString = char.ToLowerInvariant(s[0]) + (s.Length >= 2 ? s.Substring(1) : string.Empty);
                return SyntaxFactory.Identifier(newString);
            }

            return identifier;
        }
    }
}
