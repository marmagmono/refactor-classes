using System.Collections.Generic;
using System.Linq;
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
            Parameter(type, IdentifierToken(identifier));

        public static ParameterSyntax Parameter(TypeSyntax type, SyntaxToken identifier) =>
            SF.Parameter(
                EmptyAttributeList(),
                EmptyModifierList(),
                type,
                identifier,
                default(EqualsValueClauseSyntax));

        public static SyntaxToken IdentifierToken(string name) => SF.Identifier(name);

        public static IdentifierNameSyntax Identifier(string name) => SF.IdentifierName(name);

        public static SyntaxList<T> List<T>(T node) where T : SyntaxNode =>
            SF.List<T>(Enumerable.Repeat(node, 1));

        public static SyntaxList<AttributeListSyntax> EmptyAttributeList() =>
            SF.List<AttributeListSyntax>();

        public static SyntaxList<TypeParameterConstraintClauseSyntax> EmptyParameterConstraintList() =>
            SF.List<TypeParameterConstraintClauseSyntax>();

        public static SyntaxTokenList EmptyModifierList() =>
            SF.TokenList();

        public static string UppercaseFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

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
