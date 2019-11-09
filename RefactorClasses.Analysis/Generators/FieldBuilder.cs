using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;

    public class FieldBuilder
    {
        private readonly TypeSyntax fieldType;
        private List<SyntaxToken> modifiers = new List<SyntaxToken>();
        private List<string> variableNames = new List<string>();

        public FieldBuilder(TypeSyntax type)
        {
            this.fieldType = type;
        }

        public FieldBuilder AddVariables(params string[] name)
        {
            this.variableNames.AddRange(name);
            return this;
        }

        public FieldBuilder Modifiers(params SyntaxToken[] modifier)
        {
            modifiers.AddRange(modifier);
            return this;
        }

        public FieldDeclarationSyntax Build() =>
            SF.FieldDeclaration(
                GeneratorHelper.EmptyAttributeList(),
                SF.TokenList(this.modifiers),
                SF.VariableDeclaration(
                fieldType,
                SF.SeparatedList(
                    this.variableNames.Select(n => SF.VariableDeclarator(n)))));
    }
}
