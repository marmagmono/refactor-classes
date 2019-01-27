using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class ConstructorGenerationHelper
    {
        public static ConstructorDeclarationSyntax FromPropertiesWithAssignments(
            SyntaxToken identifier,
            IList<PropertyDeclarationSyntax> properties)
        {
            var parameterList = properties.ToParameterList();
            var propertyAssignments = properties.Select(
                p => SyntaxFactory.ExpressionStatement(
                    ExpressionGenerationHelper.SimpleAssignment(
                    p.Identifier, SyntaxHelpers.LowercaseIdentifierFirstLetter(p.Identifier))));

            return SyntaxFactory.ConstructorDeclaration(
                SyntaxHelpers.EmptyAttributeList(),
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                identifier.WithoutTrivia(),
                parameterList,
                default(ConstructorInitializerSyntax),
                SyntaxFactory.Block(propertyAssignments));
        }

        public static ConstructorDeclarationSyntax FromParameterList(
            SyntaxToken identifier,
            ParameterListSyntax parameterList) =>
            SyntaxFactory.ConstructorDeclaration(
                SyntaxHelpers.EmptyAttributeList(),
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)),
                identifier.WithoutTrivia(),
                parameterList,
                default(ConstructorInitializerSyntax),
                SyntaxFactory.Block());
    }
}
