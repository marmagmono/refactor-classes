using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class MethodGenerationHelper
    {
        public static MethodGenerationBuilder Builder(string identifier) =>
            Builder(SyntaxHelpers.Identifier(identifier));

        public static MethodGenerationBuilder Builder(SyntaxToken identifier) =>
            new MethodGenerationBuilder(identifier);

        public class MethodGenerationBuilder
        {
            private SyntaxToken identifier;
            private TypeSyntax returnType = Types.Void;
            private List<SyntaxToken> modifiers = new List<SyntaxToken>();
            private List<ParameterSyntax> parameters = new List<ParameterSyntax>();
            private BlockSyntax blockBody;
            private ArrowExpressionClauseSyntax expressionBody;
            private TypeParameterListSyntax typeParameters = null;
            private List<TypeParameterConstraintClauseSyntax> typeConstraints;

            public MethodGenerationBuilder(SyntaxToken identifier)
            {
                this.identifier = identifier;
            }

            public MethodGenerationBuilder ReturnType(TypeSyntax typeSyntax)
            {
                returnType = typeSyntax;
                return this;
            }

            public MethodGenerationBuilder Modifiers(params SyntaxToken[] modifier)
            {
                modifiers.AddRange(modifier);
                return this;
            }

            public MethodGenerationBuilder Parameters(params ParameterSyntax[] parameter)
            {
                parameters.AddRange(parameter);
                return this;
            }

            public MethodGenerationBuilder Body(BlockSyntax blockSyntax)
            {
                blockBody = blockSyntax;
                return this;
            }

            public MethodGenerationBuilder ArrowBody(ArrowExpressionClauseSyntax arrowExpression)
            {
                expressionBody = arrowExpression;
                return this;
            }

            public MethodDeclarationSyntax Build()
            {
                var methodDeclaration = SyntaxFactory.MethodDeclaration(
                    SyntaxHelpers.EmptyAttributeList(),
                    SyntaxFactory.TokenList(modifiers),
                    returnType,
                    default(ExplicitInterfaceSpecifierSyntax),
                    identifier,
                    typeParameters,
                    SyntaxHelpers.ParameterList(parameters),
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
        }
    }
}
