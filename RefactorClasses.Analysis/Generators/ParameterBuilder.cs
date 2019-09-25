using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;
    using GH = GeneratorHelper;

    public class ParameterBuilder
    {
        private TypeSyntax type;
        private SyntaxToken identifier;

        private List<SyntaxToken> modifiers;

        public ParameterBuilder AddModifiers(params SyntaxToken[] modifiers)
        {
            if (this.modifiers == null)
            {
                this.modifiers = new List<SyntaxToken>(modifiers);
                return this;
            }

            this.modifiers.AddRange(modifiers);
            return this;
        }

        public ParameterBuilder Type(TypeSyntax type)
        {
            this.type = type;
            return this;
        }

        public ParameterBuilder Identifier(SyntaxToken identifier)
        {
            this.identifier = identifier;
            return this;
        }

        public ParameterSyntax Build()
        {
            return SF.Parameter(
                GH.EmptyAttributeList(),
                SF.TokenList(this.modifiers ?? Enumerable.Empty<SyntaxToken>()),
                this.type,
                this.identifier,
                default(EqualsValueClauseSyntax));
        }
    }
}
