using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class PropertyDeclarationConversions
    {
        public static ParameterListSyntax ToParameterList(
            this IEnumerable<PropertyDeclarationSyntax> properties) =>
            SyntaxHelpers.ParameterList(properties.Select(ToParameter));

        public static ParameterSyntax ToParameter(
            this PropertyDeclarationSyntax propertyDeclaration) =>
            SyntaxHelpers.Parameter(
                propertyDeclaration.Type,
                SyntaxHelpers.LowercaseIdentifierFirstLetter(propertyDeclaration.Identifier));

        public static ArgumentSyntax ToArgument(
            this PropertyDeclarationSyntax propertyDeclaration) =>
            SyntaxHelpers.ArgumentFromIdentifier(propertyDeclaration.Identifier);
    }
}
