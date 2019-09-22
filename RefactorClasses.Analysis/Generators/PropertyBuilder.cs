using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;

    public sealed class PropertyBuilder
    {
        public enum PropertyType
        {
            ReadonlyGet,
        }

        private readonly TypeSyntax type;
        private readonly string identifier;
        private readonly List<SyntaxToken> modifiers = new List<SyntaxToken>();
        private PropertyType propertyType = PropertyType.ReadonlyGet;

        public PropertyBuilder(TypeSyntax type, string identifier)
        {
            this.type = type;
            this.identifier = identifier;
        }

        public PropertyBuilder AddModifiers(params SyntaxToken[] modifier)
        {
            modifiers.AddRange(modifier);
            return this;
        }

        public PropertyDeclarationSyntax Build(PropertyType propertyType = PropertyType.ReadonlyGet)
        {
            if (this.propertyType == PropertyType.ReadonlyGet)
            {
                return SF.PropertyDeclaration(
                    GeneratorHelper.EmptyAttributeList(),
                    SF.TokenList(this.modifiers),
                    this.type,
                    default(ExplicitInterfaceSpecifierSyntax),
                    GeneratorHelper.IdentifierToken(this.identifier),
                    SF.AccessorList(
                        GeneratorHelper.List(
                            SF.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration))),
                    default(ArrowExpressionClauseSyntax),
                    default(EqualsValueClauseSyntax)
                    );
            }

            throw new NotImplementedException();
        }
    }
}
