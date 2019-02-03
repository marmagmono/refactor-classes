using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    using SF = SyntaxFactory;

    public static class SyntaxHelpers
    {
        public static SyntaxList<AttributeListSyntax> EmptyAttributeList() =>
            SF.List<AttributeListSyntax>();

        public static SyntaxTokenList EmptyModifierList() =>
            SF.TokenList();

        public static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> parameters) =>
            SF.ParameterList(
                SF.Token(SyntaxKind.OpenParenToken),
                SF.SeparatedList(parameters),
                SF.Token(SyntaxKind.CloseParenToken));

        public static ParameterListSyntax ParameterList(
            IEnumerable<ParameterSyntax> parameters,
            IEnumerable<SyntaxToken> separators) =>
            SF.ParameterList(
                SF.Token(SyntaxKind.OpenParenToken),
                SF.SeparatedList(parameters, separators),
                SF.Token(SyntaxKind.CloseParenToken));

        public static BaseListSyntax BaseList(params IdentifierNameSyntax[] baseNames) =>
            SF.BaseList(SF.SeparatedList(baseNames.Select(n => SF.SimpleBaseType(n)).Cast<BaseTypeSyntax>()));

        public static SeparatedSyntaxList<TNode> SeparatedSyntaxList<TNode>(params TNode[] nodes) where TNode : SyntaxNode =>
            SF.SeparatedList(nodes);

        public static SyntaxList<TNode> List<TNode>(params TNode[] nodes) where TNode : SyntaxNode =>
            SF.List(nodes);

        public static SyntaxToken Identifier(string name) => SF.Identifier(name);

        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier) =>
            SF.Parameter(
                EmptyAttributeList(),
                EmptyModifierList(),
                type,
                identifier,
                default(EqualsValueClauseSyntax));

        public static ArgumentSyntax ArgumentFromIdentifier(SyntaxToken identifier) =>
            SF.Argument(SF.IdentifierName(identifier.WithoutTrivia()));

        public static SyntaxToken LowercaseIdentifierFirstLetter(SyntaxToken identifier)
        {
            ThrowHelpers.ThrowIfNotIdentifier(nameof(identifier), identifier);

            if (identifier.Value is string s && s.Length >= 1)
            {
                var newString = char.ToLowerInvariant(s[0]) + (s.Length >= 2 ? s.Substring(1) : string.Empty);
                return SF.Identifier(newString);
            }

            return identifier;
        }

        public static SyntaxToken UppercaseIdentifierFirstLetter(SyntaxToken identifier)
        {
            ThrowHelpers.ThrowIfNotIdentifier(nameof(identifier), identifier);

            if (identifier.Value is string s && s.Length >= 1)
            {
                var newString = char.ToUpperInvariant(s[0]) + (s.Length >= 2 ? s.Substring(1) : string.Empty);
                return SF.Identifier(newString);
            }

            return identifier;
        }
    }
}
