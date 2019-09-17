using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;

    public static class GeneratorHelper
    {
        public static ParameterListSyntax ParameterList(IEnumerable<ParameterSyntax> parameters) =>
            SF.ParameterList(
                SF.Token(SyntaxKind.OpenParenToken),
                SF.SeparatedList(parameters),
                SF.Token(SyntaxKind.CloseParenToken));

        public static ParameterSyntax Parameter(TypeSyntax type, string identifier) =>
            Parameter(type, Identifier(identifier));

        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier) =>
            SF.Parameter(
                EmptyAttributeList(),
                EmptyModifierList(),
                type,
                identifier,
                default(EqualsValueClauseSyntax));

        public static SyntaxToken Identifier(string name) => SF.Identifier(name);

        public static SyntaxList<AttributeListSyntax> EmptyAttributeList() =>
            SF.List<AttributeListSyntax>();

        public static SyntaxList<TypeParameterConstraintClauseSyntax> EmptyParameterConstraintList() =>
            SF.List<TypeParameterConstraintClauseSyntax>();

        public static SyntaxTokenList EmptyModifierList() =>
            SF.TokenList();

        public static SyntaxToken LowercaseIdentifierFirstLetter(SyntaxToken identifier)
        {
            if (identifier.Value is string s && s.Length >= 1)
            {
                var newString = char.ToLowerInvariant(s[0]) + (s.Length >= 2 ? s.Substring(1) : string.Empty);
                return SF.Identifier(newString);
            }

            return identifier;
        }
    }
}
