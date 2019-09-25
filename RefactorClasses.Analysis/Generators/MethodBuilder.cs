using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.DeclarationGeneration;
using RefactorClasses.Analysis.Syntax;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;
    using GH = GeneratorHelper;

    public sealed class MethodBuilder
    {
        private SyntaxToken identifier;
        private TypeSyntax returnType = Types.Void;
        private List<SyntaxToken> modifiers = new List<SyntaxToken>();
        private List<ParameterSyntax> parameters = new List<ParameterSyntax>();
        private BlockSyntax blockBody;
        private ArrowExpressionClauseSyntax expressionBody;
        private TypeParameterListSyntax typeParameters = null;
        private List<TypeParameterConstraintClauseSyntax> typeConstraints;

        public MethodBuilder(SyntaxToken identifier)
        {
            this.identifier = identifier;
        }

        public MethodBuilder ReturnType(TypeSyntax typeSyntax)
        {
            returnType = typeSyntax;
            return this;
        }

        public MethodBuilder Modifiers(params SyntaxToken[] modifier)
        {
            modifiers.AddRange(modifier);
            return this;
        }

        public MethodBuilder AddParameter(TypeSyntax typeSyntax, SyntaxToken identifier)
        {
            Parameters(
                SyntaxFactory.Parameter(
                    GH.EmptyAttributeList(),
                    SF.TokenList(),
                    typeSyntax,
                    identifier,
                    default(EqualsValueClauseSyntax)));
            return this;
        }

        public MethodBuilder Parameters(params ParameterSyntax[] parameter)
        {
            parameters.AddRange(parameter);
            return this;
        }

        public MethodBuilder Body(BlockSyntax blockSyntax)
        {
            blockBody = blockSyntax;
            return this;
        }

        public MethodBuilder ArrowBody(ArrowExpressionClauseSyntax arrowExpression)
        {
            expressionBody = arrowExpression;
            return this;
        }

        public MethodDeclarationSyntax Build()
        {
            var methodDeclaration = SyntaxFactory.MethodDeclaration(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(modifiers),
                returnType,
                default(ExplicitInterfaceSpecifierSyntax),
                identifier,
                typeParameters,
                GeneratorHelper.ParameterList(parameters),
                typeConstraints != null ?
                    SyntaxFactory.List(typeConstraints)
                    : SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                blockBody,
                expressionBody);

            if (expressionBody != null
                || (expressionBody == null && blockBody == null))
            {
                methodDeclaration = methodDeclaration.WithSemicolonToken(Tokens.Semicolon);
            }

            return methodDeclaration;
        }

        public ConstructorDeclarationSyntax BuildConstructor()
        {
            return SyntaxFactory.ConstructorDeclaration(
                SyntaxFactory.List<AttributeListSyntax>(),
                SyntaxFactory.TokenList(modifiers),
                identifier,
                GeneratorHelper.ParameterList(parameters),
                default(ConstructorInitializerSyntax),
                blockBody,
                expressionBody);
        }
    }
}
