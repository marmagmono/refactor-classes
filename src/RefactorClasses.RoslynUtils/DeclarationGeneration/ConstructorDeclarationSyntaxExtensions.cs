using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class ConstructorDeclarationSyntaxExtensions
    {
        public const int AppendPosition = int.MaxValue;

        /// <summary>
        /// Inserts parameter into a parameter list at <paramref name="position"/>.
        /// </summary>
        /// <remarks>It tries to keep current parameter indent.</remarks>
        /// <param name="constructorDeclaration">Constructor declaration.</param>
        /// <param name="parameter">Parameter to be inserted.</param>
        /// <param name="position">Positiona at which <paramref name="parameter"/> should be added to constructor.</param>
        /// <returns>Constructor declaration with additional parameter</returns>
        public static ConstructorDeclarationSyntax InsertParameter(
            this ConstructorDeclarationSyntax constructorDeclaration,
            ParameterSyntax parameter,
            int position)
        {
            var parameterList = constructorDeclaration.ParameterList.Parameters.ToList();
            var firstParameter = parameterList.FirstOrDefault();

            var separators = constructorDeclaration
                .ParameterList.Parameters.GetSeparators()
                .ToList();

            // Indent like the first parameter
            var parameterToInsert = parameter.WithType(
                firstParameter != null ?
                    parameter.Type.WithLeadingTrivia(firstParameter.GetLeadingTrivia())
                    : parameter.Type);

            // using separators.First() rather than comma, should preserve EOL if there is any after parameter.
            separators.Add(parameterList.Count == 1 ? Tokens.Comma : separators.First());

            if (position == AppendPosition)
            {
                parameterList.Add(parameterToInsert);
            }
            else
            {
                parameterList.Insert(position, parameterToInsert);
            }

            var parameterListSyntax = SyntaxFactory.ParameterList(
                constructorDeclaration.ParameterList.OpenParenToken, // this should preserve original trivia and EOF of there is any
                SyntaxFactory.SeparatedList(parameterList, separators),
                SyntaxFactory.Token(SyntaxKind.CloseParenToken));

            return constructorDeclaration.WithParameterList(parameterListSyntax); ;
        }


    }
}
